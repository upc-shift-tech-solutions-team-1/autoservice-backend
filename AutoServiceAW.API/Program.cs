using System.Text;
using AutoServiceAW.API.CustomerManagement.Application.Internal;
using AutoServiceAW.API.CustomerManagement.Domain.Repositories;
using AutoServiceAW.API.CustomerManagement.Domain.Services;
using AutoServiceAW.API.CustomerManagement.Infrastructure.Persistence.EFC.Repositories;
using AutoServiceAW.API.FleetManagement.Application.Internal;
using AutoServiceAW.API.FleetManagement.Domain.Repositories;
using AutoServiceAW.API.FleetManagement.Domain.Services;
using AutoServiceAW.API.FleetManagement.Infrastructure.Persistence.EFC.Repositories;
using AutoServiceAW.API.IAM.Application.Internal;
using AutoServiceAW.API.IAM.Domain.Repositories;
using AutoServiceAW.API.IAM.Domain.Services;
using AutoServiceAW.API.IAM.Infrastructure.Persistence.EFC.Repositories;
using AutoServiceAW.API.InventoryManagement.Application.Internal;
using AutoServiceAW.API.InventoryManagement.Domain.Repositories;
using AutoServiceAW.API.InventoryManagement.Domain.Services;
using AutoServiceAW.API.InventoryManagement.Infrastructure.Persistence.EFC.Repositories;
using AutoServiceAW.API.Shared.Domain.Repositories;
using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using AutoServiceAW.API.StaffCoordination.Application.Internal;
using AutoServiceAW.API.StaffCoordination.Domain.Repositories;
using AutoServiceAW.API.StaffCoordination.Domain.Services;
using AutoServiceAW.API.StaffCoordination.Infrastructure.Persistence.EFC.Repositories;
using AutoServiceAW.API.TenantManagement.Application.Internal;
using AutoServiceAW.API.TenantManagement.Domain.Repositories;
using AutoServiceAW.API.TenantManagement.Domain.Services;
using AutoServiceAW.API.TenantManagement.Infrastructure.Persistence.EFC.Repositories;
using AutoServiceAW.API.WorkshopOperations.Application.Internal;
using AutoServiceAW.API.WorkshopOperations.Domain.Repositories;
using AutoServiceAW.API.WorkshopOperations.Domain.Services;
using AutoServiceAW.API.WorkshopOperations.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

/// <summary>
/// The main entry point configuration file for the Web Application.
/// Sets up the dependency injection container, authentication infrastructure middleware, multi-tenant database context connections, and HTTP pipelines.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

#region Controller and API Explorer Configuration

// Add framework service controller endpoints to the DI container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

// Configure minimal API and controller descriptive route explorer mechanisms.
builder.Services.AddEndpointsApiExplorer();
/*builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
});*/
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Enforce uniform lower-case formatting policies on all REST endpoint routes and URL structures.
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

#endregion

#region Database Connection Configuration

// Extract the relational primary database connection string setting sequence.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (connectionString != null)
        options.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString)
        );
});

#endregion

#region Dependency Injection Registrations

// Shared Context Boundary Infrastructure Registrations
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Staff Coordination Bounded Context Registrations
builder.Services.AddScoped<IMechanicRepository, MechanicRepository>();
builder.Services.AddScoped<IMechanicService, MechanicService>();

// Inventory Management Bounded Context Registrations
builder.Services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
builder.Services.AddScoped<IInventoryItemService, InventoryItemService>();

// Customer Management Bounded Context Registrations
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Fleet Management Bounded Context Registrations
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

// Workshop Operations Bounded Context Registrations
builder.Services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
builder.Services.AddScoped<IWorkOrderService, WorkOrderService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

// Identity and Access Management (IAM) Bounded Context Registrations
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Tenant Management Bounded Context Registrations
builder.Services.AddScoped<IWorkshopRepository, WorkshopRepository>();
builder.Services.AddScoped<IWorkshopService, WorkshopService>();

#endregion

#region Security and Authentication Infrastructure

// Extract JWT token validation configurations properties.
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey =
                    new SymmetricSecurityKey(key)
            };
    });

// Configure open cross-origin sharing policies to authorize client UI system components integrations.
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
    );
});

#endregion

#region Middleware Execution Pipeline

var app = builder.Build();

// Enable Swagger API testing documentation panels under development environment contexts boundaries.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Inject security routing filters, CORS evaluations, and request authentications checkpoints to the active pipeline execution flow.
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Expose mapping routes matching operational controller boundaries.
app.MapControllers();

/*
 * Database migrations are executed only when explicitly enabled
 * through the Database:ApplyMigrationsOnStartup configuration.
 *
 * The default value is false when the property does not exist,
 * preventing accidental changes to an external or production database.
 */
var applyMigrationsOnStartup =
    builder.Configuration.GetValue<bool>(
        "Database:ApplyMigrationsOnStartup"
    );

if (applyMigrationsOnStartup)
{
    using var scope = app.Services.CreateScope();

    var services = scope.ServiceProvider;

    try
    {
        var context =
            services.GetRequiredService<AppDbContext>();

        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger =
            services.GetRequiredService<ILogger<Program>>();

        logger.LogError(
            ex,
            "An error occurred while applying migrations at startup."
        );
    }
}

// Execute the async background web runner container.
app.Run();

#endregion