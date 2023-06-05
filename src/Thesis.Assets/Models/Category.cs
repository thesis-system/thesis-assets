namespace Thesis.Assets.Models;

public class Category
{
    public Guid Id { get; set; }
    
    public Guid IntegrationId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public Guid AreaId { get; set; }
    
    public List<Asset> Assets { get; set; } = new();
}