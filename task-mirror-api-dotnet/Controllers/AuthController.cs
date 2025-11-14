using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMirror.Auth;
using TaskMirror.Data;

namespace TaskMirror.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly TaskMirrorDbContext _ctx;
        private readonly TokenService _tokenService;

        public AuthController(TaskMirrorDbContext ctx, TokenService tokenService)
        {
            _ctx = ctx;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        [AllowAnonymous] // 🔓 Login não exige JWT
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _ctx.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
                return Unauthorized("Usuário não encontrado.");

            // aqui você pode depois trocar por hash se quiser
            if (user.Password != request.Password)
                return Unauthorized("Senha inválida.");

            var token = _tokenService.GenerateToken(user);

            return Ok(new LoginResponse
            {
                Token = token,
                Role = user.RoleUsuario,
                IdUsuario = user.IdUsuario
            });
        }
    }
}
