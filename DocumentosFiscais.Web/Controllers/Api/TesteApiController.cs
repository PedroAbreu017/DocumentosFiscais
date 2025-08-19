using Microsoft.AspNetCore.Mvc;

namespace DocumentosFiscais.Web.Controllers.Api
{
    [ApiController]
    [Route("api/teste")]
    public class TesteApiController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "Funcionou!", timestamp = DateTime.Now });
        }
        
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { ping = "pong" });
        }
    }
}
