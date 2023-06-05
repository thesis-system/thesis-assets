using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Thesis.Assets.Contracts;
using Thesis.Assets.Models;
using Thesis.Assets.Options;

namespace Thesis.Assets.Controllers;

/// <summary>
/// Контроллер ассетов
/// </summary>
[ApiController]
[Route("[controller]")]
public class AssetsController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly DatabaseContext _context;
    private readonly IOptions<IntegrationOptions> _options;

    /// <summary>
    /// Конструктор класса <see cref="AssetsController"/>
    /// </summary>
    /// <param name="context">Контекст базы данных</param>
    /// <param name="options">Настройки интеграции с сервисом ассетов</param>
    public AssetsController(DatabaseContext context,IOptions<IntegrationOptions> options)
    {
        _httpClient = new HttpClient();
        _context = context;
        _options = options;
    }

    [HttpPost]
    public async Task<IActionResult> UpdateFromAssetsService()
    {
        var response = await _httpClient.GetFromJsonAsync<List<AssetDto>>($"{_options.Value.AssetsHost}/api/assets");
        if (response is null)
            return NoContent();

        var categories = response.Select(asset => asset.Category)
            .GroupBy(category => category.Id)
            .Select(group => group.First())
            .Where(category => _options.Value.Categories.Contains(category.Name))
            .Select(category => new Category
            {
                Id = Guid.NewGuid(),
                IntegrationId = category.Id,
                Name = category.Name,
                AreaId = category.AreaId
            }).ToList();

        await UpdateCategories(categories);

        var updatedCategories = _context.Categories.ToList();
        var assets = response
            .Where(asset => updatedCategories.Any(category => category.IntegrationId == asset.Category.Id))
            .Select(asset => new Asset
            {
                Id = Guid.NewGuid(),
                IntegrationId = asset.Id,
                Name = asset.Name,
                AreaId = asset.AreaId,
                CategoryId = updatedCategories.First(category => category.IntegrationId == asset.Category.Id).Id,
                Parents = asset.Parents
            }).ToList();
        
        await UpdateAssets(assets);
        
        return NoContent();
    }

    private async Task UpdateCategories(IReadOnlyCollection<Category> categories)
    {
        var categoriesIds = categories.Select(category => category.IntegrationId).ToList();
        var categoriesIdsFromDb = _context.Categories.Select(category => category.IntegrationId).ToList();
        
        var categoriesIdsToCreate = categoriesIds.Except(categoriesIdsFromDb).ToList();
        if (categoriesIdsToCreate.Any())
        {
            var categoriesToCreate = categories.Where(category => categoriesIdsToCreate.Contains(category.IntegrationId)).ToList();
            _context.Categories.AddRange(categoriesToCreate);
            await _context.SaveChangesAsync();
        }
        
        var categoriesIdsToDelete = categoriesIdsFromDb.Except(categoriesIds).ToList();
        if (categoriesIdsToDelete.Any())
        {
            var categoriesToDelete = _context.Categories.Where(category => categoriesIdsToDelete.Contains(category.IntegrationId)).ToList();
            _context.Categories.RemoveRange(categoriesToDelete);
            await _context.SaveChangesAsync();
        }
        
        var categoriesIdsToUpdate = categoriesIds.Intersect(categoriesIdsFromDb).ToList();
        if (categoriesIdsToUpdate.Any())
        {
            var categoriesToUpdateById = categories
                .Where(category => categoriesIdsToUpdate.Contains(category.IntegrationId))
                .ToDictionary(category => category.IntegrationId);
            var categoriesToUpdateFromDb = _context.Categories
                .Where(category => categoriesIdsToUpdate.Contains(category.IntegrationId))
                .ToList();

            foreach (var category in categoriesToUpdateFromDb)
            {
                var categoryToUpdate = categoriesToUpdateById[category.IntegrationId];
                category.Name = categoryToUpdate.Name;
                category.AreaId = categoryToUpdate.AreaId;
            }
            
            _context.Categories.UpdateRange(categoriesToUpdateFromDb);
            await _context.SaveChangesAsync();
        }
    }
    
    private async Task UpdateAssets(IReadOnlyCollection<Asset> assets)
    {
        var assetsIds = assets.Select(asset => asset.IntegrationId).ToList();
        var assetsIdsFromDb = _context.Assets.Select(asset => asset.IntegrationId).ToList();
        
        var assetsIdsToCreate = assetsIds.Except(assetsIdsFromDb).ToList();
        if (assetsIdsToCreate.Any())
        {
            var assetsToCreate = assets.Where(asset => assetsIdsToCreate.Contains(asset.IntegrationId)).ToList();
            _context.Assets.AddRange(assetsToCreate);
            await _context.SaveChangesAsync();
        }
        
        var assetsIdsToDelete = assetsIdsFromDb.Except(assetsIds).ToList();
        if (assetsIdsToDelete.Any())
        {
            var assetsToDelete = _context.Assets.Where(asset => assetsIdsToDelete.Contains(asset.IntegrationId)).ToList();
            _context.Assets.RemoveRange(assetsToDelete);
            await _context.SaveChangesAsync();
        }
        
        var assetsIdsToUpdate = assetsIds.Intersect(assetsIdsFromDb).ToList();
        if (assetsIdsToUpdate.Any())
        {
            var assetsToUpdateById = assets
                .Where(asset => assetsIdsToUpdate.Contains(asset.IntegrationId))
                .ToDictionary(asset => asset.IntegrationId);
            var assetsToUpdateFromDb = _context.Assets
                .Where(asset => assetsIdsToUpdate.Contains(asset.IntegrationId))
                .ToList();

            foreach (var asset in assetsToUpdateFromDb)
            {
                var assetToUpdate = assetsToUpdateById[asset.IntegrationId];
                asset.Name = assetToUpdate.Name;
                asset.AreaId = assetToUpdate.AreaId;
                asset.CategoryId = assetToUpdate.CategoryId;
                asset.Parents = assetToUpdate.Parents;
            }
            
            _context.Assets.UpdateRange(assetsToUpdateFromDb);
            await _context.SaveChangesAsync();
        }
    }
}