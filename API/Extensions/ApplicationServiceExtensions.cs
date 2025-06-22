using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignalR;
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
            // whenever you need the Interface pass the class instead, 
            // e.g. passing in IUserRepositary userRepository => allows for UserRepository functionality
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ILikesRepository, LikesRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IPhotoRepository, PhotoRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<LogUserActivity>();

            // sets our default mapper to the one specificed through our AutoMapperProfiles.cs
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            services.AddSignalR();
            // always be available and shared when application is running
            services.AddSingleton<PresenceTracker>();


            return services;

        }
    }
}
