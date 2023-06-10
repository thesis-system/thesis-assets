using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Thesis.Assets.Contracts;
using Thesis.Assets.Models;

namespace Thesis.Assets.Controllers;

/// <summary>
/// Контроллер ассетов
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AssetsController : ControllerBase
{
    private readonly DatabaseContext _context;

    /// <summary>
    /// Конструктор класса <see cref="AssetsController"/>
    /// </summary>
    /// <param name="context">Контекст базы данных</param>
    public AssetsController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAssetInfo(Guid id)
    {
        var asset = await _context.Assets
            .Include(asset => asset.Category)
            .FirstOrDefaultAsync(asset => asset.Id == id);
        
        if (asset == null) return NotFound("Актив не найден");
        
        var assetDto = new AssetDto
        {
            Id = asset.Id,
            Name = asset.Name,
            AreaId = asset.AreaId,
            Parents = asset.Parents,
            Category = new CategoryDto
            {
                Id = asset.Category.Id,
                Name = asset.Category.Name,
                AreaId = asset.Category.AreaId
            },
            Latitude = asset.Latitude,
            Longitude = asset.Longitude
        };
        return Ok(assetDto);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetStructureByApartments([FromQuery] ICollection<Guid> apartments)
    {
        if (!apartments.Any()) return BadRequest("Не указаны идентификаторы квартир");
        apartments = apartments.Distinct().ToList();
        
        var notFoundApartments = apartments
            .Where(apartment => !_context.Assets.Any(asset => asset.Id == apartment && asset.Category.Name == "Квартира"))
            .ToList();

        if (notFoundApartments.Any())
            return BadRequest($"Не найдены квартиры с идентификаторами: {string.Join(", ", notFoundApartments)}"); 
        
        var assetById = await _context.Assets
            .ToDictionaryAsync(asset => asset.Id, asset => asset);

        var apartmentsParentsIdsById = await _context.Assets
            .Where(asset => apartments.Contains(asset.Id))
            .ToDictionaryAsync(asset => asset.Id, asset => asset.Parents);

        var complexes = BuildTrees(assetById, apartmentsParentsIdsById).ToList();
        var groundsCategoryIds = await _context.Categories
            .Where(category => category.Name == "Спортивная площадка" || category.Name == "Детская площадка")
            .Select(category => category.Id)
            .ToListAsync();
        foreach (var complex in complexes)
        {
            var complexGrounds = _context.Assets
                .Include(asset => asset.Category)
                .Where(asset => asset.Parents.Contains(complex.Id) && groundsCategoryIds.Contains(asset.CategoryId))
                .ToList();
            
            complex.Children.AddRange(complexGrounds.Select(ground => new Node
            {
                Id = ground.Id,
                Name = ground.Name
            }));
        }

        return Ok(complexes);
    }

    #region BuildTree

    private static IEnumerable<Node> BuildTrees(IReadOnlyDictionary<Guid, Asset> assetById, IReadOnlyDictionary<Guid, List<Guid>> apartmentsParentsIdsById)
    {
        var queue = new Queue<(Node child, Node? parent)>();
        foreach (var (childId, parentIds) in apartmentsParentsIdsById)
        {
            var childNode = new Node
            {
                Id = childId,
                Name = assetById[childId].Name
            };
            foreach (var parentId in parentIds)
            {
                var existed = queue.FirstOrDefault(item => item.parent?.Id == parentId);
                if (existed.parent is not null)
                {
                    existed.parent.Children.Add(childNode);
                    continue;
                }
                
                var newNode = new Node
                {
                    Id = parentId,
                    Name = assetById[parentId].Name,
                    Children = new List<Node> {childNode}
                };
                
                queue.Enqueue((childNode, newNode));
            }
        }

        while (queue.Any())
        {
            var (node, parentNode) = queue.Dequeue();
            if (parentNode is null)
            {
                yield return node;
                continue;
            }

            var parentNodeParentIds = assetById[parentNode.Id].Parents;
            foreach (var parentId in parentNodeParentIds)
            {
                var existed = queue.FirstOrDefault(item => item.parent?.Id == parentId);
                if (existed.parent is not null)
                {
                    existed.parent.Children.Add(parentNode);
                    continue;
                }
                
                var newNode = new Node
                {
                    Id = parentId,
                    Name = assetById[parentId].Name,
                    Children = new List<Node> {parentNode}
                };
                
                queue.Enqueue((parentNode, newNode));
            }
            
            if (!queue.Any())
            {
                yield return parentNode;
            }
        }
    }

    #endregion
    
    #region Classes

    private class Node
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; } = string.Empty;

        public List<Node> Children { get; set; } = new();
    }

    #endregion
}
