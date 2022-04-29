using Contracts;
using Entities;
using LoggerService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebApiMS.Extensions
{
    public static class ServiceExtensions
    {
        // Configuring CORS (cross - origin resource sharing)
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            });
        }
        public static void ConfigureLoggerService(this IServiceCollection services)
        {
            services.AddScoped<ILoggerManager, LoggerManager>();
        }
        public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration) =>
            services.AddDbContext<Repository>(options => options.UseSqlServer(configuration.GetConnectionString("sqlConnection"), b=>b.MigrationsAssembly("WebApiMS")));
            // migration assembly is not in our main project , it is located in Entites class so we should provide our database to this method 
    }
}
