using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    [HttpGet]
    public async Task<IActionResult> GetStructureByApartments(ICollection<Guid> apartments)
    {
        if (!apartments.Any()) return BadRequest();
        
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
            var complex1 = complex;
            var complexGrounds = _context.Assets
                .Include(asset => asset.Category)
                .Where(asset => asset.Parents.Contains(complex1.Id) && groundsCategoryIds.Contains(asset.CategoryId))
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
