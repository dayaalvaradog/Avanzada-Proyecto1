using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TallerAutomotriz.Core.Entities;
using TallerAutomotriz.DataAccess.Interfaces;
using TallerAutomotriz.DataAccess.Repositories;

namespace TallerAutomotriz.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolicitudRepuestoController : ControllerBase
    {
        private readonly ISolicitudRepuesto _solicitudRepository;
        private readonly IRepuesto _repuestoRepository;
        private readonly IUsuario _usuarioRepository; // Necesario para obtener el usuario actual
        private readonly IConfiguration _configuration;

        public SolicitudRepuestoController(ISolicitudRepuesto solicitudRepository,
                                            IRepuesto repuestoRepository,
                                            IUsuario usuarioRepository,
                                            IConfiguration configuration)
        {
            _solicitudRepository = solicitudRepository;
            _repuestoRepository = repuestoRepository;
            _usuarioRepository = usuarioRepository;
            _configuration = configuration;
        }

        [HttpGet("ObtenerSolicitudes")]
        [Authorize(Roles = "Administrador, EncargadoBodega, Empleado")]
        public async Task<ActionResult<IEnumerable<SolicitudRepuesto>>> ObtenerSolicitudes()
        {
            var solicitudes = await _solicitudRepository.ObtenerSolicitudesAsync();
            return Ok(solicitudes);
        }

        [HttpGet("ObtenerSolicitudesPorIdSolicitante/{solicitanteId}")]
        [Authorize(Roles = "EncargadoBodega, Empleado")]
        public async Task<ActionResult<IEnumerable<SolicitudRepuesto>>> ObtenerSolicitudesPorIdSolicitante(int solicitanteId)
        {
            var solicitudes = await _solicitudRepository.ObtenerSolicitudesPorIdSolicitanteAsync(solicitanteId);
            foreach (var solicitud in solicitudes)
            {
                var usuario = await _usuarioRepository.ObtenerUsuarioPorIdAsync(solicitud.IdSolicitante);
                if (usuario != null)
                {
                    solicitud.Solicitante = usuario; // 
                }
                if (solicitud.IdUsuarioEntrega.HasValue)
                {
                    var usuarioEntrega = await _usuarioRepository.ObtenerUsuarioPorIdAsync(solicitud.IdUsuarioEntrega.Value);
                    if (usuarioEntrega != null)
                    {
                        solicitud.UsuarioEntrega = usuarioEntrega; // Asignar el usuario que entregó el repuesto
                    }
                }
                solicitud.Repuesto = await _repuestoRepository.ObtenerRepuestoPorIdAsync(solicitud.IdRepuesto);
            }
            return Ok(solicitudes);
        }

        [HttpGet("ObtenerSolicitudPorId/{id}")]
        [Authorize(Roles = "EncargadoBodega, Empleado")]
        public async Task<ActionResult<SolicitudRepuesto>> ObtenerSolicitudPorId(int id)
        {
            var solicitud = await _solicitudRepository.ObtenerSolicitudPorIdAsync(id);
            if (solicitud == null)
            {
                return NotFound();
            }
            return Ok(solicitud);
        }

        [HttpPost("InsertarSolicitudRepuesto")]
        [Authorize(Roles = "Empleado")]
        public async Task<ActionResult<SolicitudRepuesto>> InsertarSolicitudRepuesto([FromBody] SolicitudRepuesto solicitud)
        {
            if (solicitud.CantidadSolicitada <= 0)
            {
                return BadRequest("La cantidad solicitada debe ser mayor a cero.");
            }

            var repuesto = await _repuestoRepository.ObtenerRepuestoPorIdAsync(solicitud.IdRepuesto);
            if (repuesto == null)
            {
                return NotFound("El repuesto especificado no existe.");
            }

            if (repuesto.CantidadDisponible < solicitud.CantidadSolicitada)
            {
                return BadRequest($"No hay suficientes unidades de {repuesto.Nombre}. Disponibles: {repuesto.CantidadDisponible}");
            }

            solicitud.FechaSolicitud = DateTime.Now;
            solicitud.IdEstado = 2; 
            solicitud.Estado = "Pendiente"; 

            await _solicitudRepository.InsertarSolicitudAsync(solicitud);
            await _solicitudRepository.GuardarCambiosAsync();

            return CreatedAtAction(nameof(ObtenerSolicitudPorId), new { id = solicitud.Id }, solicitud);
        }

        [HttpPut("EntregarRepuesto/{id}")]
        [Authorize(Roles = "EncargadoBodega")]
        public async Task<IActionResult> EntregarRepuesto(int id)
        {
            var solicitud = await _solicitudRepository.ObtenerSolicitudPorIdAsync(id);
            if (solicitud == null)
            {
                return NotFound("Solicitud no encontrada.");
            }

            if (solicitud.Estado == "Entregado")
            {
                return BadRequest("La solicitud ya ha sido entregada.");
            }

            var usuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuario == null)
            {
                return Unauthorized("No se pudo identificar al usuario que realiza la entrega.");
            }
            var usuarioEntregaId = int.Parse(usuario);

            solicitud.Estado = "Entregado";
            solicitud.FechaEntrega = DateTime.Now;
            solicitud.IdUsuarioEntrega = usuarioEntregaId;

            var repuesto = await _repuestoRepository.ObtenerRepuestoPorIdAsync(solicitud.IdRepuesto);
            if (repuesto == null)
            {
                return NotFound("Repuesto asociado a la solicitud no encontrado.");
            }
            if (repuesto.CantidadDisponible < solicitud.CantidadSolicitada)
            {
                return BadRequest($"No hay suficiente cantidad disponible del repuesto '{repuesto.Nombre}' para esta solicitud. Disponibles: {repuesto.CantidadDisponible}.");
            }
            repuesto.CantidadDisponible -= solicitud.CantidadSolicitada;

            try
            {
                // Llama directamente a los métodos que ya guardan en la DB
                await _solicitudRepository.ModificarSolicitudAsync(solicitud);
                await _repuestoRepository.ModificarRepuestoAsync(repuesto);

                return NoContent(); // HTTP 204 No Content para éxito sin cuerpo de respuesta
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Error de concurrencia al actualizar la solicitud o el repuesto.");
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                // _logger.LogError(ex, "Error al entregar repuesto para solicitud {SolicitudId}", id);
                return StatusCode(500, $"Error interno del servidor al entregar repuesto: {ex.Message}");
            }
        }

        [HttpPut("RechazarSolicitud/{id}/Rechazar")]
        [Authorize(Roles = "EncargadoBodega")]
        public async Task<IActionResult> RechazarSolicitud(int id)
        {
            var solicitud = await _solicitudRepository.ObtenerSolicitudPorIdAsync(id);
            if (solicitud == null)
            {
                return NotFound("Solicitud no encontrada.");
            }

            if (solicitud.Estado == "Entregada" || solicitud.Estado == "Rechazada")
            {
                return BadRequest("La solicitud ya ha sido procesada.");
            }

            solicitud.Estado = "Rechazada";
            solicitud.FechaEntrega = DateTime.Now; // O un campo de FechaRechazo

            await _solicitudRepository.ModificarSolicitudAsync(solicitud);
            if (await _solicitudRepository.GuardarCambiosAsync())
            {
                return NoContent();
            }

            return StatusCode(500, "Error al rechazar la solicitud.");
        }
    }
}
