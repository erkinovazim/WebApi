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
                .AllowCredentials()
                .AllowAnyMethod()
                .AllowAnyHeader());
            });
        }
    }
}
