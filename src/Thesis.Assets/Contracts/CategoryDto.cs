namespace Thesis.Assets.Contracts;

public class CategoryDto
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public Guid AreaId { get; set; }
    
    public List<Guid> Parents { get; set; } = new();
}
