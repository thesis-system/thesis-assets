using Microsoft.AspNetCore.Mvc;

namespace Thesis.Assets.Controllers;

/// <summary>
/// Контроллер ассетов
/// </summary>
[ApiController]
[Route("[controller]")]
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
}
