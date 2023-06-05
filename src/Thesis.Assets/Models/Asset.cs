namespace Thesis.Assets.Models;

public class Asset
{
    public Guid Id { get; set; }
    
    public Guid IntegrationId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public Guid AreaId { get; set; }
    
    public Guid CategoryId { get; set; }
    
    public virtual Category Category { get; set; } = null!;
    
    public List<Guid> Parents { get; set; } = new();
}