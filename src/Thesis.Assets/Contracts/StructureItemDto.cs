namespace Thesis.Assets.Contracts;

/// <summary>
/// Модель структуры активов
/// </summary>
public class StructureItemDto
{
    /// <summary>
    /// Идентификатор актива
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Название актива
    /// </summary>
    public string Name { get; set; } = string.Empty;
}