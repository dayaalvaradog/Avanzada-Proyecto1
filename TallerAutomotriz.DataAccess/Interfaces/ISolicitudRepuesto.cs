using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TallerAutomotriz.Core.Entities;

namespace TallerAutomotriz.DataAccess.Interfaces
{
    public interface ISolicitudRepuesto
    {
        Task<IEnumerable<SolicitudRepuesto>> ObtenerSolicitudesAsync();
        Task<IEnumerable<SolicitudRepuesto>> ObtenerSolicitudesPorIdSolicitanteAsync(int solicitanteId);
        Task<SolicitudRepuesto> ObtenerSolicitudPorIdAsync(int id);
        Task InsertarSolicitudAsync(SolicitudRepuesto solicitud);
        Task ModificarSolicitudAsync(SolicitudRepuesto solicitud);
        Task EliminarSolicitudAsync(int id);
        Task<bool> GuardarCambiosAsync();
    }
}
