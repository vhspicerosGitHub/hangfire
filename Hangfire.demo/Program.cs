using Hangfire;
using Hangfire.demo.Handlers;
using Hangfire.Storage.SQLite;
using HangfireBasicAuthenticationFilter;
using Serilog;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
                loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

        builder.Services.AddHangfire(opt =>
        {
            opt.
            SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseColouredConsoleLogProvider()
            .UseSimpleAssemblyNameTypeSerializer()
            .UseSQLiteStorage()
            //.UseInMemoryStorage()
            .UseRecommendedSerializerSettings();
        });


        var app = builder.Build();
        app.UseHangfireServer(new BackgroundJobServerOptions
        {
            ServerName = app.Configuration.GetSection("HangfireSettings:ServerName").Value
        });

        app.UseHttpsRedirection();

        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[]
            {
        new HangfireCustomBasicAuthenticationFilter
        {
            User = app.Configuration.GetSection("HangfireSettings:Username").Value,
            Pass = app.Configuration.GetSection("HangfireSettings:Password").Value
            }
                }
        });

        RecurringJob.AddOrUpdate<UsuarioHandler>("UsuarioHandler", x => x.Execute(), Cron.Hourly);

        app.UseAuthorization();

        app.MapControllers();

        app.Run();


    }
}