using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgValoniaGPS.Models;

namespace AgValoniaGPS.Services.Interfaces;

/// <summary>
/// Service for field management (loading, saving, calculations)
/// </summary>
public interface IFieldService
{
    /// <summary>
    /// Event fired when the active field changes
    /// </summary>
    event EventHandler<Field?>? ActiveFieldChanged;

    /// <summary>
    /// Current active field
    /// </summary>
    Field? ActiveField { get; }

    /// <summary>
    /// Load field from file
    /// </summary>
    Task<Field?> LoadFieldAsync(string filePath);

    /// <summary>
    /// Save field to file
    /// </summary>
    Task SaveFieldAsync(Field field, string filePath);

    /// <summary>
    /// Get list of available fields
    /// </summary>
    Task<List<string>> GetFieldListAsync();

    /// <summary>
    /// Set the active field
    /// </summary>
    void SetActiveField(Field? field);

    /// <summary>
    /// Calculate field area from boundaries
    /// </summary>
    double CalculateArea(List<Position> points);

    /// <summary>
    /// Check if a position is inside a boundary
    /// </summary>
    bool IsPositionInsideBoundary(Position position, Boundary boundary);
}