using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TallerAutomotriz.Core.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }
        public string HashContrasena { get; set; } 
        public string Rol { get; set; } 
        public bool Activo { get; set; } = true;
    }
}
