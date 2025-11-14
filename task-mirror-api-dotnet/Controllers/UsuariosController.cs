using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMirror.Data;
using TaskMirror.DTOs;
using TaskMirror.Models;
using TaskMirror.Services;

namespace TaskMirror.Controllers;

[ApiController]
[Route("api/v1/usuarios")]
[Authorize(Roles = "LIDER")] // 🔒 SOMENTE LIDER pode mexer em usuários
public class UsuariosController : ControllerBase
{
    private readonly TaskMirrorDbContext _db;
    private readonly IMapper _mapper;

    public UsuariosController(TaskMirrorDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    // GET api/v1/usuarios
    [HttpGet]
    public async Task<ActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        var query = _db.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.IdUsuario)
            .AsQueryable();

        var total = await query.CountAsync();

        var list = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dto = _mapper.Map<IEnumerable<UsuarioDto>>(list);

        var result = new
        {
            data = dto,
            total,
            page,
            pageSize,
            _links = Hateoas.BuildListLinks("/api/v1/usuarios", page, pageSize, total)
        };

        return Ok(result);
    }

    // GET api/v1/usuarios/5
    [HttpGet("{idUsuario:int}")]
    public async Task<ActionResult> GetById(int idUsuario)
    {
        var u = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdUsuario == idUsuario);

        if (u is null)
            return NotFound();

        var dto = _mapper.Map<UsuarioDto>(u);

        var result = new
        {
            data = dto,
            _links = Hateoas.BuildResourceLinks("/api/v1/usuarios", idUsuario)
        };

        return Ok(result);
    }

    // POST api/v1/usuarios
    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Post(UsuarioCreateDto dto)
    {
        var entity = _mapper.Map<Usuario>(dto);

        _db.Usuarios.Add(entity);
        await _db.SaveChangesAsync();

        var outDto = _mapper.Map<UsuarioDto>(entity);
        return CreatedAtAction(nameof(GetById), new { idUsuario = entity.IdUsuario }, outDto);
    }

    // PUT api/v1/usuarios/5
    [HttpPut("{idUsuario:int}")]
    public async Task<IActionResult> Put(int idUsuario, UsuarioUpdateDto dto)
    {
        // opcional: validar coerência se dto tiver IdUsuario
        // if (dto.IdUsuario != 0 && dto.IdUsuario != idUsuario) return BadRequest("IdUsuario do path difere do body.");

        var entity = await _db.Usuarios.FindAsync(idUsuario);
        if (entity is null) return NotFound();

        _mapper.Map(dto, entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE api/v1/usuarios/5
    [HttpDelete("{idUsuario:int}")]
    public async Task<IActionResult> Delete(int idUsuario)
    {
        var entity = await _db.Usuarios.FindAsync(idUsuario);
        if (entity is null) return NotFound();

        _db.Usuarios.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
