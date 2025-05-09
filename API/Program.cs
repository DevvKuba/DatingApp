
using Microsoft.EntityFrameworkCore;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            // database setup
            builder.Services.AddDbContext<DataContext>(opt =>
            {
                opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            // allows cross origin resource sharing
            builder.Services.AddCors();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            // specifying what can be shared and with who
            app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:63967", "https://localhost:63967"));

            app.MapControllers();

            app.Run();
        }
    }
}
