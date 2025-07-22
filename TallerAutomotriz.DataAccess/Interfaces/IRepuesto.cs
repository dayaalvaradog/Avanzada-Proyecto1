using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TallerAutomotriz.Core.Entities;

namespace TallerAutomotriz.DataAccess.Interfaces
{
    public interface IRepuesto
    {
        Task<IEnumerable<Repuesto>> ObtenerRepuestosAsync();
        Task<Repuesto> ObtenerRepuestoPorIdAsync(int id);
        Task InsertarRepuestoAsync(Repuesto repuesto);
        Task ModificarRepuestoAsync(Repuesto repuesto);
        Task EliminarRepuestoAsync(int id);
        Task<bool> GuardarCambiosAsync();
    }
}
