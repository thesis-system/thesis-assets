namespace Thesis.Assets.Contracts;

public class AssetDto
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public Guid AreaId { get; set; }

    public List<Guid> Parents { get; set; } = new();

    public CategoryDto Category { get; set; } = null!;
    
    public List<AssetParameterDto> Parameters { get; set; } = new();
}
