using Microsoft.Extensions.Options;
using Thesis.Assets.Contracts;
using Thesis.Assets.Models;
using Thesis.Assets.Options;

namespace Thesis.Assets.Services;

/// <summary>
/// Сервис обновления активов.
/// </summary>
public class AssetsUpdateService : IHostedService
{
    private const int ServiceDefaultDelay = 86400;
    private readonly IServiceProvider _services;

    private readonly TimeSpan _delay;
    private Timer _timer = null!;
    
    /// <summary>
    /// Конструктор сервиса обновления активов.
    /// </summary>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <param name="services">Провайдер сервисов.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public AssetsUpdateService(IConfiguration configuration, IServiceProvider services)
    {
        var delay = configuration.GetValue<int>("AssetsUpdateServiceDelay", ServiceDefaultDelay);
        if (delay <= 0)
            throw new ArgumentOutOfRangeException(nameof(delay), delay, "Invalid delay value.");
        
        _delay = TimeSpan.FromSeconds(delay);
        _services = services;
    }

    /// <inheritdoc cref="IHostedService.StartAsync"/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(UpdateAssets!, null, TimeSpan.Zero, _delay);
        return Task.CompletedTask;
    }

    /// <inheritdoc cref="IHostedService.StopAsync"/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    #region UpdateAssets

    private async void UpdateAssets(object state)
    {
        var httpClient = new HttpClient();
        
        using var scope = _services.CreateScope();
        var integrationOptions = scope.ServiceProvider.GetRequiredService<IOptions<IntegrationOptions>>();
        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var logger = _services.GetRequiredService<ILogger<AssetsUpdateService>>();
        logger.LogInformation($"Updating assets...");
        try
        {
            var response = await httpClient.GetFromJsonAsync<List<AssetDto>>($"{integrationOptions.Value.AssetsHost}/api/assets");
            if (response is null)
                return;

            var categories = response.Select(asset => asset.Category)
                .GroupBy(category => category.Id)
                .Select(group => group.First())
                .Where(category => integrationOptions.Value.Categories.Contains(category.Name))
                .Select(category => new Category
                {
                    Id = Guid.NewGuid(),
                    IntegrationId = category.Id,
                    Name = category.Name,
                    AreaId = category.AreaId
                }).ToList();

            logger.LogInformation($"Updating categories...");
            await UpdateCategories(categories, context);
            logger.LogInformation($"Categories updated.");

            var updatedCategories = context.Categories.ToList();
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
        
            logger.LogInformation($"Updating assets...");
            await UpdateAssets(assets, context);
            logger.LogInformation($"Assets updated.");
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Error while updating assets.");
        }
        
        logger.LogInformation($"All assets updated.");
    }

    private static async Task UpdateCategories(IReadOnlyCollection<Category> categories, DatabaseContext context)
    {
        var categoriesIds = categories.Select(category => category.IntegrationId).ToList();
        var categoriesIdsFromDb = context.Categories.Select(category => category.IntegrationId).ToList();
        
        var categoriesIdsToCreate = categoriesIds.Except(categoriesIdsFromDb).ToList();
        if (categoriesIdsToCreate.Any())
        {
            var categoriesToCreate = categories.Where(category => categoriesIdsToCreate.Contains(category.IntegrationId)).ToList();
            context.Categories.AddRange(categoriesToCreate);
            await context.SaveChangesAsync();
        }
        
        var categoriesIdsToDelete = categoriesIdsFromDb.Except(categoriesIds).ToList();
        if (categoriesIdsToDelete.Any())
        {
            var categoriesToDelete = context.Categories.Where(category => categoriesIdsToDelete.Contains(category.IntegrationId)).ToList();
            context.Categories.RemoveRange(categoriesToDelete);
            await context.SaveChangesAsync();
        }
        
        var categoriesIdsToUpdate = categoriesIds.Intersect(categoriesIdsFromDb).ToList();
        if (categoriesIdsToUpdate.Any())
        {
            var categoriesToUpdateById = categories
                .Where(category => categoriesIdsToUpdate.Contains(category.IntegrationId))
                .ToDictionary(category => category.IntegrationId);
            var categoriesToUpdateFromDb = context.Categories
                .Where(category => categoriesIdsToUpdate.Contains(category.IntegrationId))
                .ToList();

            foreach (var category in categoriesToUpdateFromDb)
            {
                var categoryToUpdate = categoriesToUpdateById[category.IntegrationId];
                category.Name = categoryToUpdate.Name;
                category.AreaId = categoryToUpdate.AreaId;
            }
            
            context.Categories.UpdateRange(categoriesToUpdateFromDb);
            await context.SaveChangesAsync();
        }
    }
    
    private static async Task UpdateAssets(IReadOnlyCollection<Asset> assets, DatabaseContext context)
    {
        var assetsIds = assets.Select(asset => asset.IntegrationId).ToList();
        var assetsIdsFromDb = context.Assets.Select(asset => asset.IntegrationId).ToList();
        
        var assetsIdsToCreate = assetsIds.Except(assetsIdsFromDb).ToList();
        if (assetsIdsToCreate.Any())
        {
            var assetsToCreate = assets.Where(asset => assetsIdsToCreate.Contains(asset.IntegrationId)).ToList();
            context.Assets.AddRange(assetsToCreate);
            await context.SaveChangesAsync();
        }
        
        var assetsIdsToDelete = assetsIdsFromDb.Except(assetsIds).ToList();
        if (assetsIdsToDelete.Any())
        {
            var assetsToDelete = context.Assets.Where(asset => assetsIdsToDelete.Contains(asset.IntegrationId)).ToList();
            context.Assets.RemoveRange(assetsToDelete);
            await context.SaveChangesAsync();
        }
        
        var assetsIdsToUpdate = assetsIds.Intersect(assetsIdsFromDb).ToList();
        if (assetsIdsToUpdate.Any())
        {
            var assetsToUpdateById = assets
                .Where(asset => assetsIdsToUpdate.Contains(asset.IntegrationId))
                .ToDictionary(asset => asset.IntegrationId);
            var assetsToUpdateFromDb = context.Assets
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
            
            context.Assets.UpdateRange(assetsToUpdateFromDb);
            await context.SaveChangesAsync();
        }
    }

    #endregion
}