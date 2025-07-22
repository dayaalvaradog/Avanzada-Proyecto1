using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TallerAutomotriz.Core.Entities;

namespace TallerAutomotriz.DataAccess.Interfaces
{
    public interface IUsuario
    {
        Task<IEnumerable<Usuario>> ObtenerUsuariosAsync();
        Task<Usuario> ObtenerUsuarioPorIdAsync(int id);
        Task<Usuario> ObtenerUsuarioPorCorreoAsync(string correo);
        Task InsertarUsuarioAsync(Usuario usuario);
        Task ModificarUsuarioAsync(Usuario usuario);
        Task EliminarUsuarioAsync(int id);
        Task<bool> GuardarCambiosAsync();
    }
}
