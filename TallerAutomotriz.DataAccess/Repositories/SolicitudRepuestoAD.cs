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
    public class SolicitudRepuestoAD : ISolicitudRepuesto
    {
        private readonly ApplicationDbContext _context;

        public SolicitudRepuestoAD(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SolicitudRepuesto>> ObtenerSolicitudesAsync()
        {
            return await _context.SolicitudesRepuesto
                .Include(s => s.Repuesto)
                .Include(s => s.Solicitante)
                .Include(s => s.UsuarioEntrega)
                .ToListAsync();
        }

        public async Task<IEnumerable<SolicitudRepuesto>> ObtenerSolicitudesPorIdSolicitanteAsync(int solicitanteId)
        {
            return await _context.SolicitudesRepuesto
                .Where(s => s.IdSolicitante == solicitanteId)
                .Include(s => s.Repuesto)
                .ToListAsync();
        }

        public async Task<SolicitudRepuesto> ObtenerSolicitudPorIdAsync(int id)
        {
            return await _context.SolicitudesRepuesto
                .Include(s => s.Repuesto)
                .Include(s => s.Solicitante)
                .Include(s => s.UsuarioEntrega)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task InsertarSolicitudAsync(SolicitudRepuesto solicitud)
        {
            await _context.SolicitudesRepuesto.AddAsync(solicitud);
        }

        public async Task ModificarSolicitudAsync(SolicitudRepuesto solicitud)
        {
            _context.SolicitudesRepuesto.Update(solicitud);
        }

        public async Task EliminarSolicitudAsync(int id)
        {
            var solicitud = await _context.SolicitudesRepuesto.FindAsync(id);
            if (solicitud != null)
            {
                _context.SolicitudesRepuesto.Remove(solicitud);
            }
        }

        public async Task<bool> GuardarCambiosAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
