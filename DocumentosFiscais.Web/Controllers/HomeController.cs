using DocumentosFiscais.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocumentosFiscais.Web.Controllers;

public class HomeController : Controller
{
    private readonly IDocumentoService _documentoService;
    
    public HomeController(IDocumentoService documentoService)
    {
        _documentoService = documentoService;
    }
    
    public async Task<IActionResult> Index()
    {
        var statsResult = await _documentoService.GetDashboardStatsAsync();
        
        if (!statsResult.Success)
        {
            ViewBag.ErrorMessage = statsResult.ErrorMessage;
            return View(new DashboardStats());
        }
        
        return View(statsResult.Data);
    }
    
    public IActionResult About()
    {
        ViewBag.Message = "Sistema de Gestão de Documentos Fiscais Eletrônicos";
        ViewBag.Description = "Desenvolvido para demonstrar competências técnicas em ASP.NET MVC, Entity Framework e Clean Architecture.";
        
        return View();
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}

