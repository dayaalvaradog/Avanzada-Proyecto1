using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TallerAutomotriz.Core.Entities
{
    public class SolicitudRepuesto
    {
        public int Id { get; set; }
        public int IdRepuesto { get; set; }
        public Repuesto? Repuesto { get; set; } 
        public int IdSolicitante { get; set; } 
        public Usuario? Solicitante { get; set; } 
        public int CantidadSolicitada { get; set; }
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;
        public int IdEstado { get; set; } = 2;
        public string Estado { get; set; } = "Pendiente";
        public DateTime? FechaEntrega { get; set; } 
        public int? IdUsuarioEntrega { get; set; } 
        public Usuario? UsuarioEntrega { get; set; }
    }
}
