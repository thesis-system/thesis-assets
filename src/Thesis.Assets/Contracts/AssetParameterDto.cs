namespace Thesis.Assets.Contracts;

/// <summary>
/// Модель данных для параметра актива
/// </summary>
public class AssetParameterDto
{
    /// <summary>
    /// Идентификатор параметра актива
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Название параметра актива
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Модель данных для координат актива
    /// </summary>
    public CoordinatesDto Coordinates { get; set; } = null!;
}
