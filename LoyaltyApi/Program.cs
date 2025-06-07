using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using LoyaltyApi.Data;
using LoyaltyApi.Middleware;

namespace LoyaltyApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configurar Entity Framework Core
            builder.Services.AddDbContext<LoyaltyContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("LoyaltyDb")));

            // Configurar Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Loyalty API", Version = "v1" });
                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Description = "API Key needed to access the endpoints. Add 'X-API-Key' in the header with your key.",
                    In = ParameterLocation.Header,
                    Name = "X-API-Key",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "ApiKey"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            builder.Services.AddControllers();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication(); // Agregado para compatibilidad futura, aunque no se usa activamente
            app.UseMiddleware<ApiKeyMiddleware>(); // Usar el middleware personalizado para validar API Key
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}