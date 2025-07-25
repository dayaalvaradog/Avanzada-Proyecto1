using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TallerAutomotriz.Core.Entities;
using TallerAutomotriz.Core.Models;
using TallerAutomotriz.DataAccess.Interfaces;

namespace TallerAutomotriz.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuario _usuarioRepository;
        private readonly IConfiguration _configuration;

        public UsuarioController(IUsuario usuarioRepository, IConfiguration configuration)
        {
            _usuarioRepository = usuarioRepository;
            _configuration = configuration;
        }

        [HttpGet("ObtenerUsuarios")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<IEnumerable<Usuario>>> ObtenerUsuarios()
        {
            var usuarios = await _usuarioRepository.ObtenerUsuariosAsync();
            return Ok(usuarios);
        }

        [HttpGet("ObtenerUsuarioPorId/{id}")]
        [Authorize]
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
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<Usuario>> InsertarUsuario([FromBody] Usuario usuario)
        {
            if (await _usuarioRepository.ObtenerUsuarioPorCorreoAsync(usuario.Correo) != null)
            {
                return Conflict("El correo electrónico ya está registrado.");
            }

            // Hashear la contraseña antes de guardar
            usuario.HashContrasena = BCrypt.Net.BCrypt.HashPassword(usuario.HashContrasena);
            
            await _usuarioRepository.InsertarUsuarioAsync(usuario);
            await _usuarioRepository.GuardarCambiosAsync();

            return CreatedAtAction(nameof(ObtenerUsuarioPorId), new { id = usuario.Id }, usuario);
        }

        [HttpPost("RegistrarUsuario")]
        [AllowAnonymous]
        public async Task<ActionResult<Usuario>> RegistrarUsuario([FromBody] Usuario usuario)
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
        [Authorize]
        public async Task<IActionResult> ModificarUsuario(int id, [FromBody] Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return BadRequest("El ID de la ruta no coincide con el ID del usuario en el cuerpo.");
            }

            var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rolusuario = User.FindFirst(ClaimTypes.Role)?.Value;

            // Obtener el usuario existente para comparar el ID y mantener el hash de la contraseña
            var usuarioExistente = await _usuarioRepository.ObtenerUsuarioPorIdAsync(id);
            if (usuarioExistente == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            if (rolusuario != "Administrador" && (idUsuarioClaim == null || int.Parse(idUsuarioClaim) != id))
            {
                return Forbid("No tiene permiso para modificar este perfil.");
            }

            if (rolusuario != "Administrador")
            {
                usuarioExistente.Nombre = usuario.Nombre;
                usuarioExistente.Apellido = usuario.Apellido;
                usuarioExistente.Correo = usuario.Correo;
            }
            else // Es un administrador
            {
                usuarioExistente.Nombre = usuario.Nombre;
                usuarioExistente.Apellido = usuario.Apellido;
                usuarioExistente.Correo = usuario.Correo;
                usuarioExistente.Rol = usuario.Rol; 

                if (!string.IsNullOrEmpty(usuario.HashContrasena) && (usuario.HashContrasena != usuarioExistente.HashContrasena))
                {
                    usuarioExistente.HashContrasena = BCrypt.Net.BCrypt.HashPassword(usuario.HashContrasena);
                }
            }

            var existeEmail = await _usuarioRepository.ObtenerUsuarioPorCorreoAsync(usuario.Correo);
            if (existeEmail != null && existeEmail.Id != usuarioExistente.Id)
            {
                return Conflict("El correo electrónico ya está en uso por otro usuario.");
            }

            try
            {
                await _usuarioRepository.ModificarUsuarioAsync(usuarioExistente);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _usuarioRepository.ObtenerUsuarioPorIdAsync(id) == null)
                {
                    return NotFound("Error de concurrencia: El usuario ya no existe.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // 204 No Content
        }

        [HttpDelete("EliminarUsuario/{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var usuario = await _usuarioRepository.ObtenerUsuarioPorIdAsync(id);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            // Opcional: Impedir que un admin se elimine a sí mismo
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim != null && int.Parse(userIdClaim) == id)
            {
                return BadRequest("Un administrador no puede eliminarse a sí mismo.");
            }

            await _usuarioRepository.EliminarUsuarioAsync(id);
            return NoContent();
        }

        [HttpPost("login")]
        [AllowAnonymous] 
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _usuarioRepository.ObtenerUsuarioPorCorreoAsync(model.Correo);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Contrasena, user.HashContrasena))
            {
                return Unauthorized(new { message = "Credenciales inválidas." });
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Correo),
                new Claim(ClaimTypes.Name, $"{user.Nombre} {user.Apellido}"),
                new Claim(ClaimTypes.Role, user.Rol) // <<-- Añadir el rol como un claim
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"] ?? "7")); 

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return Ok(new AuthResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Id = user.Id,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Correo = user.Correo,
                Rol = user.Rol
            });
        }

        [HttpGet("perfil")]
        [Authorize] 
        public async Task<ActionResult<Usuario>> ObtenerPerfilUsuarioActual()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("No se pudo identificar al usuario autenticado.");
            }

            var userId = int.Parse(userIdClaim);
            var usuario = await _usuarioRepository.ObtenerUsuarioPorIdAsync(userId);

            if (usuario == null)
            {
                return NotFound("Perfil de usuario no encontrado.");
            }
            return Ok(usuario);
        }
    }
}
