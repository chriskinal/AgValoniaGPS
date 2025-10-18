namespace AgValoniaGPS.Models.Guidance;

/// <summary>
/// Defines the unit system for distance measurements and guidance calculations.
/// </summary>
public enum UnitSystem
{
    /// <summary>
    /// Metric system using meters for distance measurements.
    /// </summary>
    Metric,

    /// <summary>
    /// Imperial system using feet for distance measurements.
    /// </summary>
    Imperial
}

/// <summary>
/// Provides utility methods for converting between unit systems.
/// All internal calculations are performed in meters.
/// </summary>
public static class UnitSystemExtensions
{
    /// <summary>
    /// Conversion factor from meters to feet.
    /// </summary>
    private const double MetersToFeetFactor = 3.28084;

    /// <summary>
    /// Conversion factor from feet to meters.
    /// </summary>
    private const double FeetToMetersFactor = 0.3048;

    /// <summary>
    /// Converts a distance value from meters to feet.
    /// </summary>
    /// <param name="meters">The distance in meters.</param>
    /// <returns>The distance in feet.</returns>
    public static double MetersToFeet(double meters)
    {
        return meters * MetersToFeetFactor;
    }

    /// <summary>
    /// Converts a distance value from feet to meters.
    /// </summary>
    /// <param name="feet">The distance in feet.</param>
    /// <returns>The distance in meters.</returns>
    public static double FeetToMeters(double feet)
    {
        return feet * FeetToMetersFactor;
    }

    /// <summary>
    /// Converts a distance value to meters based on the specified unit system.
    /// </summary>
    /// <param name="value">The distance value to convert.</param>
    /// <param name="unitSystem">The unit system of the input value.</param>
    /// <returns>The distance in meters.</returns>
    public static double ToMeters(double value, UnitSystem unitSystem)
    {
        return unitSystem switch
        {
            UnitSystem.Metric => value,
            UnitSystem.Imperial => FeetToMeters(value),
            _ => value
        };
    }

    /// <summary>
    /// Converts a distance value from meters to the specified unit system.
    /// </summary>
    /// <param name="meters">The distance in meters.</param>
    /// <param name="unitSystem">The target unit system.</param>
    /// <returns>The distance in the specified unit system.</returns>
    public static double FromMeters(double meters, UnitSystem unitSystem)
    {
        return unitSystem switch
        {
            UnitSystem.Metric => meters,
            UnitSystem.Imperial => MetersToFeet(meters),
            _ => meters
        };
    }

    /// <summary>
    /// Gets the abbreviation for the unit system.
    /// </summary>
    /// <param name="unitSystem">The unit system.</param>
    /// <returns>The unit abbreviation ("m" for metric, "ft" for imperial).</returns>
    public static string GetUnitAbbreviation(this UnitSystem unitSystem)
    {
        return unitSystem switch
        {
            UnitSystem.Metric => "m",
            UnitSystem.Imperial => "ft",
            _ => "m"
        };
    }

    /// <summary>
    /// Gets the full name of the unit for display purposes.
    /// </summary>
    /// <param name="unitSystem">The unit system.</param>
    /// <returns>The unit name ("meters" for metric, "feet" for imperial).</returns>
    public static string GetUnitName(this UnitSystem unitSystem)
    {
        return unitSystem switch
        {
            UnitSystem.Metric => "meters",
            UnitSystem.Imperial => "feet",
            _ => "meters"
        };
    }
}
