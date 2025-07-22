using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TallerAutomotriz.Core.Entities;
using TallerAutomotriz.DataAccess.Interfaces;

namespace TallerAutomotriz.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepuestoController : ControllerBase
    {
        private readonly IRepuesto _repuestoRepository;

        public RepuestoController(IRepuesto repuestoRepository)
        {
            _repuestoRepository = repuestoRepository;
        }

        [HttpGet("ObtenerRepuestos")]
        public async Task<ActionResult<IEnumerable<Repuesto>>> ObtenerRepuestos()
        {
            var repuestos = await _repuestoRepository.ObtenerRepuestosAsync();
            return Ok(repuestos);
        }

        [HttpGet("ObtenerRepuestoPorId/{id}")]
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

            await _repuestoRepository.ModificarRepuestoAsync(repuestoExistente);
            if (await _repuestoRepository.GuardarCambiosAsync())
            {
                return NoContent();
            }
            return StatusCode(500, "Error al actualizar el repuesto.");
        }
    }
}
