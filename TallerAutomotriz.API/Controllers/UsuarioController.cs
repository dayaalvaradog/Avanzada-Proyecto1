using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TallerAutomotriz.Core.Entities;
using TallerAutomotriz.DataAccess.Interfaces;

namespace TallerAutomotriz.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuario _usuarioRepository;

        public UsuarioController(IUsuario usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        [HttpGet("ObtenerUsuarios")]
        public async Task<ActionResult<IEnumerable<Usuario>>> ObtenerUsuarios()
        {
            var usuarios = await _usuarioRepository.ObtenerUsuariosAsync();
            return Ok(usuarios);
        }

        [HttpGet("ObtenerUsuarioPorId/{id}")]
        public async Task<ActionResult<Usuario>> ObtenerUsuarioPorId(int id)
        {
            var usuario = await _usuarioRepository.ObtenerUsuarioPorIdAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return Ok(usuario);
        }

        [HttpPost("InsertarUsuario")]
        public async Task<ActionResult<Usuario>> InsertarUsuario([FromBody] Usuario usuario)
        {
            if (await _usuarioRepository.ObtenerUsuarioPorCorreoAsync(usuario.Correo) != null)
            {
                return Conflict("El correo electrónico ya está registrado.");
            }

            // Hashear la contraseña antes de guardar
            usuario.HashContrasena = BCrypt.Net.BCrypt.HashPassword(usuario.HashContrasena);
            usuario.Rol = usuario.Rol ?? "Empleado";

            await _usuarioRepository.InsertarUsuarioAsync(usuario);
            await _usuarioRepository.GuardarCambiosAsync();

            return CreatedAtAction(nameof(ObtenerUsuarioPorId), new { id = usuario.Id }, usuario);
        }

        [HttpPut("ModificarUsuario/{id}")]
        public async Task<IActionResult> ModificarUsuario(int id, [FromBody] Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return BadRequest("El ID del usuario no coincide.");
            }

            var existingUser = await _usuarioRepository.ObtenerUsuarioPorIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            existingUser.Nombre = usuario.Nombre;
            existingUser.Apellido = usuario.Apellido;
            existingUser.Correo = usuario.Correo;
            existingUser.Activo = usuario.Activo;

            await _usuarioRepository.ModificarUsuarioAsync(existingUser);
            if (await _usuarioRepository.GuardarCambiosAsync())
            {
                return NoContent(); // 204 No Content para actualización exitosa
            }
            return StatusCode(500, "Error al actualizar el usuario.");
        }

        [HttpPost("login")]
        public async Task<ActionResult<Usuario>> Login([FromBody] Login model)
        {
            var usuario = await _usuarioRepository.ObtenerUsuarioPorCorreoAsync(model.Correo);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.Contrasena, usuario.HashContrasena))
            {
                return Unauthorized("Credenciales inválidas.");
            }

            usuario.HashContrasena = null;
            return Ok(usuario);
        }
    }
}
