namespace AgValoniaGPS.Models.FieldOperations;

/// <summary>
/// Specifies the type of U-turn pattern to execute
/// </summary>
public enum UTurnType
{
    /// <summary>
    /// Standard Ω turn (180° curved path using Dubins algorithm)
    /// </summary>
    Omega,

    /// <summary>
    /// T-turn (forward, reverse, forward pattern with manual nudging capability)
    /// </summary>
    T,

    /// <summary>
    /// Y-turn (angled turn with reduced reversing distance)
    /// </summary>
    Y
}
