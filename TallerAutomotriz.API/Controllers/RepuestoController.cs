using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TallerAutomotriz.Core.Entities;
using TallerAutomotriz.DataAccess.Interfaces;

namespace TallerAutomotriz.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepuestoController : ControllerBase
    {
        private readonly IRepuesto _repuestoRepository;
        private readonly IConfiguration _configuration;

        public RepuestoController(IRepuesto repuestoRepository, IConfiguration configuration)
        {
            _repuestoRepository = repuestoRepository;   
            _configuration = configuration;
        }

        [HttpGet("ObtenerRepuestos")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Repuesto>>> ObtenerRepuestos()
        {
            var repuestos = await _repuestoRepository.ObtenerRepuestosAsync();
            return Ok(repuestos);
        }

        [HttpGet("ObtenerRepuestoPorId/{id}")]
        [Authorize]
        public async Task<ActionResult<Repuesto>> ObtenerRepuestoPorId(int id)
        {
            var repuesto = await _repuestoRepository.ObtenerRepuestoPorIdAsync(id);
            if (repuesto == null)
            {
                return NotFound();
            }
            return Ok(repuesto);
        }

        [HttpPost("InsertarRepuesto")]
        [Authorize(Roles = "EncargadoBodega")]
        public async Task<ActionResult<Repuesto>> InsertarRepuesto([FromBody] Repuesto repuesto)
        {
            if (repuesto.CantidadDisponible < 0 || repuesto.PrecioUnitario < 0)
            {
                return BadRequest("La cantidad disponible y el precio unitario no pueden ser negativos.");
            }

            await _repuestoRepository.InsertarRepuestoAsync(repuesto);
            await _repuestoRepository.GuardarCambiosAsync();

            return CreatedAtAction(nameof(ObtenerRepuestoPorId), new { id = repuesto.Id }, repuesto);
        }

        [HttpPut("ModificarRepuesto/{id}")]
        [Authorize]
        public async Task<IActionResult> ModificarRepuesto(int id, [FromBody] Repuesto repuesto)
        {
            if (id != repuesto.Id)
            {
                return BadRequest("El ID del repuesto no coincide.");
            }

            var repuestoExistente = await _repuestoRepository.ObtenerRepuestoPorIdAsync(id);
            if (repuestoExistente == null)
            {
                return NotFound();
            }

            repuestoExistente.Nombre = repuesto.Nombre;
            repuestoExistente.Descripcion = repuesto.Descripcion;
            repuestoExistente.NumeroParte = repuesto.NumeroParte;
            repuestoExistente.CantidadDisponible = repuesto.CantidadDisponible;
            repuestoExistente.PrecioUnitario = repuesto.PrecioUnitario;
            repuestoExistente.Ubicacion = repuesto.Ubicacion;

            try
            {
                await _repuestoRepository.ModificarRepuestoAsync(repuestoExistente);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _repuestoRepository.ObtenerRepuestoPorIdAsync(id) == null)
                {
                    return NotFound("Error de concurrencia: El repuesto ya no existe.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // 204 No Content
        }
    }
}
