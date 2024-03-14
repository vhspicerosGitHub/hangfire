using Dapper;
using Hangfire.demo.Dtos;
using System.Data.SqlClient;    


namespace Hangfire.demo.Handlers
{
    public class UsuarioHandler
    {
        private readonly SqlConnection _connectionFrom;
        private readonly SqlConnection _connectionTo;
        protected readonly IConfiguration _config;
        protected readonly ILogger<UsuarioHandler> _logger;

        public UsuarioHandler(IConfiguration config, ILogger<UsuarioHandler> logger)
        {
            _config = config;
            _logger = logger;
            _connectionFrom = new SqlConnection(_config.GetConnectionString("ConnectionFrom"));
            _connectionTo = new SqlConnection(_config.GetConnectionString("ConnectionTo"));
        }

        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        public async Task Execute()
        {
            _logger.LogInformation("Iniciando UsuarioHandler");
            try
            {
                var usuariosLegacy = await _connectionFrom.QueryAsync<UsuarioLegacy>("SELECT id,nombre,correo From usuarios where traspasado = 0 or traspasado is null");
                _logger.LogInformation($"Cantidad de usuarios a transpasar => ${usuariosLegacy.Count()} ");
                var hoy = DateTime.Now;
                foreach (var item in usuariosLegacy)
                {
                    _connectionTo.Execute("INSERT usuarios (nombre,correo, id_legacy,fecha_traspaso) values (@nombre,@correo,@id_legacy,@fecha_traspaso)",
                        new { nombre = item.Nombre, correo = item.Correo, id_legacy = item.Id, fecha_traspaso = hoy }
                        );

                    _connectionFrom.Execute("UPDATE usuarios set traspasado = 1, fecha_traspaso = @fecha_traspaso  where id= @id",
                        new { id = item.Id, fecha_traspaso = hoy }
                        );
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                throw e;
            }
            finally
            {
                _logger.LogInformation("Terminando UsuarioHandler");
            }
        }
    }
}
