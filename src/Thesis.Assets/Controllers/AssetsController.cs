using Microsoft.AspNetCore.Authorization;
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
[Authorize]
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

    /// <summary>
    /// Получить информацию об активе по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор актива</param>
    /// <response code="200">Информация об активе</response>
    /// <response code="401">Токен доступа истек</response>
    /// <response code="404">Актив не найден</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AssetDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssetInfo([FromRoute] Guid id)
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
    
    /// <summary>
    /// Получить информацию об структуре по идентификаторам квартир
    /// </summary>
    /// <param name="apartmentIds">Идентификаторы квартир</param>
    /// <response code="200">Структура собственности</response>
    /// <response code="400">Не указаны идентификаторы квартир</response>
    /// <response code="401">Токен доступа истек</response>
    /// <response code="404">Квартиры не найдены</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Node>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStructureByApartments([FromQuery] ICollection<Guid> apartmentIds)
    {
        if (!apartmentIds.Any()) return BadRequest("Не указаны идентификаторы квартир");
        apartmentIds = apartmentIds.Distinct().ToList();
        
        var notFoundApartments = apartmentIds
            .Where(apartment => !_context.Assets.Any(asset => asset.Id == apartment && asset.Category.Name == "Квартира"))
            .ToList();

        if (notFoundApartments.Any())
            return NotFound($"Не найдены квартиры с идентификаторами: {string.Join(", ", notFoundApartments)}"); 
        
        var assetById = await _context.Assets
            .ToDictionaryAsync(asset => asset.Id, asset => asset);

        var apartmentsParentsIdsById = await _context.Assets
            .Where(asset => apartmentIds.Contains(asset.Id))
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
