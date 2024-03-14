using Microsoft.AspNetCore.Routing.Constraints;

namespace Hangfire.demo.Dtos
{
    public record UsuarioLegacy
    {
        public int Id { get; set; }
        public string Nombre { set; get; } = string.Empty;
        public string Correo{ set; get; } = string.Empty;
    }
}
