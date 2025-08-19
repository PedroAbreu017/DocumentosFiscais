using Microsoft.AspNetCore.Mvc;

namespace DocumentosFiscais.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "API funcionando!", timestamp = DateTime.Now });
    }
}