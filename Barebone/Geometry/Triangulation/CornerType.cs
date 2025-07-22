namespace BareBone.Geometry.Triangulation;

internal enum CornerType
{
    /// <summary>
    /// Less than 180 degrees.
    /// </summary>
    Inflex,
    /// <summary>
    /// Exactly 180 or exactly 0 degrees.
    /// </summary>
    StraightOrZero,
    /// <summary>
    /// More than 180 degrees.
    /// </summary>
    Reflex
}