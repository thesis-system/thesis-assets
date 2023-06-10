namespace Thesis.Assets.Models;

/// <summary>
/// Модель данных для актива
/// </summary>
public class Asset
{
    /// <summary>
    /// Идентификатор актива
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Название актива
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Идентификатор области
    /// </summary>
    public Guid AreaId { get; set; }
    
    /// <summary>
    /// Идентификатор категории актива
    /// </summary>
    public Guid CategoryId { get; set; }
    
    /// <summary>
    /// Категория актива
    /// </summary>
    public virtual Category Category { get; set; } = null!;
    
    /// <summary>
    /// Идентификаторы родительских активов
    /// </summary>
    public List<Guid> Parents { get; set; } = new();
    
    /// <summary>
    /// Широта
    /// </summary>
    public double Latitude { get; set; }
    
    /// <summary>
    /// Долгота
    /// </summary>
    public double Longitude { get; set; }
}