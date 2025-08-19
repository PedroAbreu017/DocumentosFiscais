using DocumentosFiscais.Core.Services;
using DocumentosFiscais.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace DocumentosFiscais.Web.Controllers.Api;

[ApiController]
[Route("api/documentos")]
public class DocumentosApiController : ControllerBase
{
    private readonly IDocumentoService _documentoService;

    public DocumentosApiController(IDocumentoService documentoService)
    {
        _documentoService = documentoService;
    }
    
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { 
            message = "API Funcionando!", 
            controller = "DocumentosApi",
            timestamp = DateTime.Now 
        });
    }

    /// <summary>
    /// Retorna lista paginada de documentos fiscais
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<DocumentoFiscal>>>> GetDocumentos(
        int page = 1,
        int pageSize = 10,
        string? filtro = null,
        TipoDocumento? tipo = null,
        StatusProcessamento? status = null)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _documentoService.GetPagedAsync(page, pageSize, filtro, tipo, status);

            if (!result.Success)
            {
                return BadRequest(new ApiResponse<PagedResult<DocumentoFiscal>>
                {
                    Success = false,
                    Message = result.ErrorMessage
                });
            }

            var response = new ApiResponse<PagedResult<DocumentoFiscal>>
            {
                Success = true,
                Data = result.Data,
                Message = "Documentos recuperados com sucesso"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<PagedResult<DocumentoFiscal>>
            {
                Success = false,
                Message = $"Erro interno: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Retorna um documento fiscal específico por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DocumentoFiscal>>> GetDocumento(int id)
    {
        try
        {
            var result = await _documentoService.GetByIdAsync(id);

            if (!result.Success)
            {
                return NotFound(new ApiResponse<DocumentoFiscal>
                {
                    Success = false,
                    Message = result.ErrorMessage
                });
            }

            var response = new ApiResponse<DocumentoFiscal>
            {
                Success = true,
                Data = result.Data,
                Message = "Documento encontrado"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<DocumentoFiscal>
            {
                Success = false,
                Message = $"Erro interno: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Upload de um novo documento fiscal via API
    /// </summary>
    [HttpPost("upload")]
    public async Task<ActionResult<ApiResponse<DocumentoFiscal>>> UploadDocumento(
        IFormFile file,
        TipoDocumento? tipo = null)
    {
        try
        {
            var result = await _documentoService.ProcessUploadAsync(file, tipo);

            if (!result.Success)
            {
                return BadRequest(new ApiResponse<DocumentoFiscal>
                {
                    Success = false,
                    Message = result.ErrorMessage
                });
            }

            var response = new ApiResponse<DocumentoFiscal>
            {
                Success = true,
                Data = result.Data,
                Message = "Documento processado com sucesso",
                Warnings = result.Warnings
            };

            return CreatedAtAction(nameof(GetDocumento), new { id = result.Data!.Id }, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<DocumentoFiscal>
            {
                Success = false,
                Message = $"Erro interno: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Exclui um documento fiscal
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDocumento(int id)
    {
        try
        {
            var result = await _documentoService.DeleteAsync(id);

            if (!result.Success)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = result.ErrorMessage
                });
            }

            var response = new ApiResponse<object>
            {
                Success = true,
                Message = "Documento excluído com sucesso"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = $"Erro interno: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Download do conteúdo XML de um documento
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadXml(int id)
    {
        try
        {
            var result = await _documentoService.GetByIdAsync(id);

            if (!result.Success)
            {
                return NotFound();
            }

            var documento = result.Data!;
            var bytes = System.Text.Encoding.UTF8.GetBytes(documento.ConteudoXml);

            return File(bytes, "application/xml", documento.NomeArquivo);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Erro interno: {ex.Message}" });
        }
    }
}
