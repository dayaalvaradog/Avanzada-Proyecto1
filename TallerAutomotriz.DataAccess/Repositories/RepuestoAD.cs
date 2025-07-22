using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TallerAutomotriz.Core.Entities;
using TallerAutomotriz.DataAccess.Data;
using TallerAutomotriz.DataAccess.Interfaces;

namespace TallerAutomotriz.DataAccess.Repositories
{
    public class RepuestoAD : IRepuesto
    {
        private readonly ApplicationDbContext _context;

        public RepuestoAD(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Repuesto>> ObtenerRepuestosAsync()
        {
            return await _context.Repuestos.ToListAsync();
        }

        public async Task<Repuesto> ObtenerRepuestoPorIdAsync(int id)
        {
            return await _context.Repuestos.FindAsync(id);
        }

        public async Task InsertarRepuestoAsync(Repuesto repuesto)
        {
            await _context.Repuestos.AddAsync(repuesto);
        }

        public async Task ModificarRepuestoAsync(Repuesto repuesto)
        {
            _context.Repuestos.Update(repuesto);
        }

        public async Task EliminarRepuestoAsync(int id)
        {
            var repuesto = await _context.Repuestos.FindAsync(id);
            if (repuesto != null)
            {
                _context.Repuestos.Remove(repuesto);
            }
        }

        public async Task<bool> GuardarCambiosAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
