namespace Thesis.Assets.Contracts;

/// <summary>
/// Модель данных для категории актива
/// </summary>
public class CategoryDto
{
    /// <summary>
    /// Идентификатор категории актива
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Название категории актива
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Идентификатор области
    /// </summary>
    public Guid AreaId { get; set; }
    
    /// <summary>
    /// Идентификаторы родительских категорий активов
    /// </summary>
    public List<Guid> Parents { get; set; } = new();
}
