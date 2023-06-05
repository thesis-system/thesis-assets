namespace Thesis.Assets.Options;

/// <summary>
/// Настройки интеграции с сервисом ассетов
/// </summary>
public class IntegrationOptions
{
    /// <summary>
    /// Разрешенные категории
    /// </summary>
    public ICollection<string> Categories { get; set; } = new List<string>();
    
    /// <summary>
    /// Хост сервиса ассетов
    /// </summary>
    public string AssetsHost { get; set; } = string.Empty;
}