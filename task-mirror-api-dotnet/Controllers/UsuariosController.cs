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
        _db = db; _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> Get()
        => Ok(_mapper.Map<IEnumerable<UsuarioDto>>(await _db.Usuarios.AsNoTracking().ToListAsync()));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> GetById(int id)
    {
        var u = await _db.Usuarios.FindAsync(id);
        return u is null ? NotFound() : Ok(_mapper.Map<UsuarioDto>(u));
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Post(UsuarioCreateDto dto)
    {
        var u = _mapper.Map<Usuario>(dto);
        _db.Usuarios.Add(u);
        await _db.SaveChangesAsync();
        var outDto = _mapper.Map<UsuarioDto>(u);
        return CreatedAtAction(nameof(GetById), new { id = u.Id }, outDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, UsuarioUpdateDto dto)
    {
        var u = await _db.Usuarios.FindAsync(id);
        if (u is null) return NotFound();
        _mapper.Map(dto, u);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var u = await _db.Usuarios.FindAsync(id);
        if (u is null) return NotFound();
        _db.Usuarios.Remove(u);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
