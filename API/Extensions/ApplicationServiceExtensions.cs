using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        // takes in IServiceCollection & IConfiguration as they are the interfaces that implement
        // follow up builder.{{method}} methods
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {

            services.AddControllers();
            // database setup
            services.AddDbContext<DataContext>(opt =>
            {
                opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });
            // allows cross origin resource sharing
            services.AddCors();
            services.AddScoped<ITokenService, TokenService>();

            return services;

        }
    }
}
