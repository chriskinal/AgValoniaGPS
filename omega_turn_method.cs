    /// <summary>
    /// Generates an Omega turn path using Dubins path generation.
    /// Matches legacy CreateABOmegaTurn and CreateCurveOmegaTurn logic from CYouTurn.cs.
    ///
    /// Algorithm (from legacy CYouTurn.cs lines 241-445 and 837-940):
    /// 1. Entry point is the end of the current track
    /// 2. Goal position is calculated based on turn offset (tool width - overlap) * rowSkips
    /// 3. Goal heading is inverted (180° from entry) for the U-turn
    /// 4. Dubins path connects entry to goal with smooth curves (RSR or LSL depending on turn direction)
    /// 5. Points are spaced based on turning radius (radius * 0.1 default in legacy)
    /// 6. Waypoints too close together are removed (distance squared < pointSpacing)
    /// 7. Boundary collision is checked and path adjusted if needed (handled by caller via BoundaryGuidedDubinsService)
    /// </summary>
    /// <param name="entryPoint">Start position of turn (end of current track)</param>
    /// <param name="entryHeading">Current heading in radians</param>
    /// <param name="exitPoint">Target position (start of next track)</param>
    /// <param name="exitHeading">Target heading in radians (typically inverted from entry for U-turn)</param>
    /// <param name="turningRadius">Vehicle turning radius in meters</param>
    /// <param name="waypointSpacing">Distance between waypoints in meters (default: radius * 0.1)</param>
    /// <returns>TurnPath with waypoints for the Omega turn</returns>
    private TurnPath GenerateOmegaTurn(
        Position2D entryPoint,
        double entryHeading,
        Position2D exitPoint,
        double exitHeading,
        double turningRadius,
        double waypointSpacing = 0.5)
    {
        // Default spacing based on turning radius (matches legacy: radius * 0.1)
        if (waypointSpacing <= 0)
        {
            waypointSpacing = turningRadius * 0.1;
        }

        // Generate Dubins path from entry to exit
        // The DubinsPathService will automatically select optimal path type:
        // - RSR (Right-Straight-Right) for right turns
        // - LSL (Left-Straight-Left) for left turns
        // - RSL, LSR for mixed turns
        // - RLR, LRL for complex scenarios
        var dubinsPath = _dubinsPathService.GeneratePath(
            entryPoint.Easting, entryPoint.Northing, entryHeading,
            exitPoint.Easting, exitPoint.Northing, exitHeading,
            turningRadius,
            waypointSpacing
        );

        var turnPath = new TurnPath(TurnStyle.Omega, entryPoint, exitPoint)
        {
            EntryHeading = entryHeading,
            ExitHeading = exitHeading,
            DubinsPath = dubinsPath
        };

        if (dubinsPath != null && dubinsPath.Waypoints.Count > 0)
        {
            // Convert Dubins waypoints to Position2D list
            var waypoints = dubinsPath.Waypoints
                .Select(w => new Position2D(w.Easting, w.Northing))
                .ToList();

            // Remove waypoints that are too close together (matches legacy point spacing cleanup)
            // Legacy code (lines 350-361 in CYouTurn.cs): removes points with distance < pointSpacing using squared distance check
            double minDistanceSquared = waypointSpacing * waypointSpacing;
            var cleanedWaypoints = new List<Position2D> { waypoints[0] }; // Always keep first point

            for (int i = 1; i < waypoints.Count - 1; i++)
            {
                var lastKept = cleanedWaypoints[cleanedWaypoints.Count - 1];
                var current = waypoints[i];

                double de = current.Easting - lastKept.Easting;
                double dn = current.Northing - lastKept.Northing;
                double distSquared = de * de + dn * dn;

                // Only keep if distance is sufficient
                if (distSquared >= minDistanceSquared)
                {
                    cleanedWaypoints.Add(current);
                }
            }

            // Always keep last point
            if (waypoints.Count > 1)
            {
                cleanedWaypoints.Add(waypoints[waypoints.Count - 1]);
            }

            turnPath.Waypoints = cleanedWaypoints;
            turnPath.TotalLength = dubinsPath.TotalLength;
        }
        else
        {
            // Fallback: create simple semicircular arc
            // This handles cases where Dubins path generation fails
            turnPath.Waypoints = GenerateSimpleOmegaWaypoints(entryPoint, entryHeading, turningRadius, waypointSpacing);
            turnPath.TotalLength = Math.PI * turningRadius; // Semicircle length
        }

        return turnPath;
    }
