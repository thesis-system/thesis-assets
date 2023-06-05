namespace Thesis.Assets.Contracts;

public class AssetParameterDto
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public CoordinatesDto Coordinates { get; set; } = null!;
}
