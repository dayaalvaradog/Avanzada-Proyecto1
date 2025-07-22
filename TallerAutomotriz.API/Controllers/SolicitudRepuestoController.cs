using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public SolicitudRepuestoController(ISolicitudRepuesto solicitudRepository,
                                            IRepuesto repuestoRepository,
                                            IUsuario usuarioRepository)
        {
            _solicitudRepository = solicitudRepository;
            _repuestoRepository = repuestoRepository;
            _usuarioRepository = usuarioRepository;
        }

        [HttpGet("ObtenerSolicitudes")]
        public async Task<ActionResult<IEnumerable<SolicitudRepuesto>>> ObtenerSolicitudes()
        {
            var solicitudes = await _solicitudRepository.ObtenerSolicitudesAsync();
            return Ok(solicitudes);
        }

        [HttpGet("PorMecanico/{solicitanteId}")]
        public async Task<ActionResult<IEnumerable<SolicitudRepuesto>>> ObtenerSolicitudesPorIdSolicitante(int solicitanteId)
        {
            var solicitudes = await _solicitudRepository.ObtenerSolicitudesPorIdSolicitanteAsync(solicitanteId);
            return Ok(solicitudes);
        }

        [HttpGet("ObtenerSolicitudPorId/{id}")]
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

        [HttpPut("EntregarRepuesto/{id}/Entregar")]
        public async Task<IActionResult> EntregarRepuesto(int id)
        {
            var solicitud = await _solicitudRepository.ObtenerSolicitudPorIdAsync(id);
            if (solicitud == null)
            {
                return NotFound("Solicitud no encontrada.");
            }

            if (solicitud.Estado == "Entregada")
            {
                return BadRequest("Esta solicitud ya ha sido entregada.");
            }

            var repuesto = await _repuestoRepository.ObtenerRepuestoPorIdAsync(solicitud.IdRepuesto);
            if (repuesto == null)
            {
                return NotFound("Repuesto asociado a la solicitud no encontrado.");
            }

            if (repuesto.CantidadDisponible < solicitud.CantidadSolicitada)
            {
                return BadRequest($"No hay suficientes unidades de {repuesto.Nombre} para completar la entrega. Disponibles: {repuesto.CantidadDisponible}");
            }

            // Actualizar inventario
            repuesto.CantidadDisponible -= solicitud.CantidadSolicitada;
            await _repuestoRepository.ModificarRepuestoAsync(repuesto);

            // Actualizar estado de la solicitud
            solicitud.IdEstado = 3; 
            solicitud.Estado = "Entregada";
            solicitud.FechaEntrega = DateTime.Now;
            solicitud.IdUsuarioEntrega = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            await _solicitudRepository.ModificarSolicitudAsync(solicitud);

            // Guardar todos los cambios transaccionalmente
            if (await _solicitudRepository.GuardarCambiosAsync() && await _repuestoRepository.GuardarCambiosAsync())
            {
                return NoContent(); // 204 No Content para actualización exitosa
            }

            return StatusCode(500, "Error al procesar la entrega del repuesto.");
        }

        [HttpPut("RechazarSolicitud/{id}/Rechazar")]
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
