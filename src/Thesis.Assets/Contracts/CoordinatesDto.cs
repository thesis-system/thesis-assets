namespace Thesis.Assets.Contracts;

/// <summary>
/// Модель данных для координат актива
/// </summary>
public class CoordinatesDto
{
    /// <summary>
    /// Широта
    /// </summary>
    public double Lat { get; set; }
    
    /// <summary>
    /// Долгота
    /// </summary>
    public double Lng { get; set; }
    
    /// <summary>
    /// ?
    /// </summary>
    public double Alt { get; set; }
}
