namespace Thesis.Assets.Contracts;

/// <summary>
/// Модель данных для актива
/// </summary>
public class AssetDto
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
    /// Идентификаторы родительских активов
    /// </summary>
    public List<Guid> Parents { get; set; } = new();

    /// <summary>
    /// Категория актива
    /// </summary>
    public CategoryDto Category { get; set; } = null!;
    
    /// <summary>
    /// Параметры актива
    /// </summary>
    public List<AssetParameterDto> Parameters { get; set; } = new();
}
