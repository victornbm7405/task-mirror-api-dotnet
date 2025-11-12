using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskMirror.Data;
using TaskMirror.DTOs;
using TaskMirror.Models;

namespace TaskMirror.Controllers;

[ApiController]
[Route("api/v1/usuarios")]
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
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> Get()
    {
        var list = await _db.Usuarios
            .AsNoTracking()
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<UsuarioDto>>(list));
    }

    // GET api/v1/usuarios/5
    [HttpGet("{idUsuario:int}")]
    public async Task<ActionResult<UsuarioDto>> GetById(int idUsuario)
    {
        var u = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdUsuario == idUsuario);

        return u is null ? NotFound() : Ok(_mapper.Map<UsuarioDto>(u));
    }

    // POST api/v1/usuarios
    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Post(UsuarioCreateDto dto)
    {
        // dto esperado: Username, Password, RoleUsuario, Funcao, IdLider (opcional)
        var entity = _mapper.Map<Usuario>(dto);
        _db.Usuarios.Add(entity);
        await _db.SaveChangesAsync();

        var outDto = _mapper.Map<UsuarioDto>(entity); // esperado: IdUsuario, Username, RoleUsuario, Funcao, IdLider
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

        _mapper.Map(dto, entity); // dto deve ter apenas campos válidos do DDL (Username, Password, RoleUsuario, Funcao, IdLider)
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
