using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TallerAutomotriz.Core.Entities
{
    public class Repuesto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string NumeroParte { get; set; }
        public int CantidadDisponible { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string Ubicacion { get; set; }
    }
}
