using System;

namespace AgValoniaGPS.Services;

/// <summary>
/// Mathematical and unit conversion constants used throughout the application.
/// All internal calculations should use meters as the base unit.
/// </summary>
public static class MathConstants
{
    #region Angular Constants

    /// <summary>
    /// Two times PI (2π ≈ 6.283185...)
    /// </summary>
    public const double TwoPI = 6.28318530717958647692;

    /// <summary>
    /// PI divided by 2 (π/2 ≈ 1.570796...)
    /// </summary>
    public const double PIBy2 = 1.57079632679489661923;

    /// <summary>
    /// Degrees to radians conversion factor (π/180 ≈ 0.017453...)
    /// </summary>
    public const double DegreesToRadians = 0.01745329251994329576923690768489;

    /// <summary>
    /// Radians to degrees conversion factor (180/π ≈ 57.295779...)
    /// </summary>
    public const double RadiansToDegrees = 57.295779513082325225835265587528;

    #endregion

    #region Length Conversions

    /// <summary>
    /// Inches to meters conversion factor
    /// </summary>
    public const double InchesToMeters = 0.0254;

    /// <summary>
    /// Meters to inches conversion factor
    /// </summary>
    public const double MetersToInches = 39.3701;

    /// <summary>
    /// Meters to feet conversion factor
    /// </summary>
    public const double MetersToFeet = 3.28084;

    /// <summary>
    /// Feet to meters conversion factor
    /// </summary>
    public const double FeetToMeters = 0.3048;

    /// <summary>
    /// Meters to miles conversion factor
    /// </summary>
    public const double MetersToMiles = 0.000621371;

    /// <summary>
    /// Miles to meters conversion factor
    /// </summary>
    public const double MilesToMeters = 1609.34;

    /// <summary>
    /// Meters to kilometers conversion factor
    /// </summary>
    public const double MetersToKilometers = 0.001;

    /// <summary>
    /// Kilometers to meters conversion factor
    /// </summary>
    public const double KilometersToMeters = 1000.0;

    #endregion

    #region Area Conversions

    /// <summary>
    /// Hectares to acres conversion factor
    /// </summary>
    public const double HectaresToAcres = 2.47105;

    /// <summary>
    /// Acres to hectares conversion factor
    /// </summary>
    public const double AcresToHectares = 0.404686;

    /// <summary>
    /// Square meters to acres conversion factor
    /// </summary>
    public const double SquareMetersToAcres = 0.000247105;

    /// <summary>
    /// Square meters to hectares conversion factor
    /// </summary>
    public const double SquareMetersToHectares = 0.0001;

    /// <summary>
    /// Acres to square meters conversion factor
    /// </summary>
    public const double AcresToSquareMeters = 4046.86;

    /// <summary>
    /// Hectares to square meters conversion factor
    /// </summary>
    public const double HectaresToSquareMeters = 10000.0;

    #endregion

    #region Volume and Flow Rate Conversions

    /// <summary>
    /// Liters to US gallons conversion factor
    /// </summary>
    public const double LitersToGallons = 0.264172;

    /// <summary>
    /// US gallons to liters conversion factor
    /// </summary>
    public const double GallonsToLiters = 3.785412534258;

    /// <summary>
    /// US gallons per acre to liters per hectare conversion factor
    /// </summary>
    public const double GallonsPerAcreToLitersPerHectare = 9.35396;

    /// <summary>
    /// Liters per hectare to US gallons per acre conversion factor
    /// </summary>
    public const double LitersPerHectareToGallonsPerAcre = 0.106907;

    #endregion

    #region Speed Conversions

    /// <summary>
    /// Meters per second to miles per hour conversion factor
    /// </summary>
    public const double MetersPerSecondToMilesPerHour = 2.23694;

    /// <summary>
    /// Meters per second to kilometers per hour conversion factor
    /// </summary>
    public const double MetersPerSecondToKilometersPerHour = 3.6;

    /// <summary>
    /// Miles per hour to meters per second conversion factor
    /// </summary>
    public const double MilesPerHourToMetersPerSecond = 0.44704;

    /// <summary>
    /// Kilometers per hour to meters per second conversion factor
    /// </summary>
    public const double KilometersPerHourToMetersPerSecond = 0.277778;

    #endregion

    #region Numerical Tolerances

    /// <summary>
    /// Small epsilon value for floating point comparisons
    /// </summary>
    public const double Epsilon = 1e-8;

    /// <summary>
    /// Larger epsilon for less precise comparisons
    /// </summary>
    public const double EpsilonLarge = 1e-6;

    #endregion
}
