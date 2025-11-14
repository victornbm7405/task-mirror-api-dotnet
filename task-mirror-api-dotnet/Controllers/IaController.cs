using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskMirror.Services;

namespace TaskMirror.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IaController : ControllerBase
    {
        private readonly OllamaIaService _iaService;

        public IaController(OllamaIaService iaService)
        {
            _iaService = iaService;
        }

        public class ChatRequest
        {
            public string Mensagem { get; set; } = string.Empty;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Mensagem))
            {
                return BadRequest(new { erro = "Campo 'mensagem' é obrigatório." });
            }

            var resposta = await _iaService.PerguntarAsync(request.Mensagem);

            return Ok(new
            {
                pergunta = request.Mensagem,
                resposta
            });
        }
    }
}
