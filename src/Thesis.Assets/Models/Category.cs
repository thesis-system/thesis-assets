namespace Thesis.Assets.Models;

/// <summary>
/// Модель данных для категории актива
/// </summary>
public class Category
{
    /// <summary>
    /// Идентификатор категории актива
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Интеграционный идентификатор категории актива
    /// </summary>
    public Guid IntegrationId { get; set; }
    
    /// <summary>
    /// Название категории актива
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Идентификатор области
    /// </summary>
    public Guid AreaId { get; set; }
    
    /// <summary>
    /// Категория активов категории
    /// </summary>
    public List<Asset> Assets { get; set; } = new();
}