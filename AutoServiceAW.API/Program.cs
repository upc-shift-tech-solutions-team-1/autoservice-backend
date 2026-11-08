
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
/// Configures dependency injection, authentication, authorization,
/// database connections, CORS, health checks and HTTP middleware.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

#region Controller and API Explorer Configuration

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddEndpointsApiExplorer();

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

builder.Services.Configure<RouteOptions>(
    options => options.LowercaseUrls = true
);

#endregion

#region Database Connection Configuration

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (connectionString != null)
    {
        options.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString)
        );
    }
});

#endregion

#region Dependency Injection Registrations

// Shared Context Boundary
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Staff Coordination
builder.Services.AddScoped<IMechanicRepository, MechanicRepository>();
builder.Services.AddScoped<IMechanicService, MechanicService>();

// Inventory Management
builder.Services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
builder.Services.AddScoped<IInventoryItemService, InventoryItemService>();

// Customer Management
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Fleet Management
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

// Workshop Operations
builder.Services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
builder.Services.AddScoped<IWorkOrderService, WorkOrderService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

// Identity and Access Management
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Tenant Management
builder.Services.AddScoped<IWorkshopRepository, WorkshopRepository>();
builder.Services.AddScoped<IWorkshopService, WorkshopService>();

#endregion

#region Security and Authentication Infrastructure

// JWT configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

builder.Services
    .AddAuthentication(options =>
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

builder.Services.AddAuthorization();

// Health checks
builder.Services.AddHealthChecks();

// CORS: allow only explicitly configured frontend origins.
// Configuration key: Cors:AllowedOrigins
// Render environment variables: Cors__AllowedOrigins__0, etc.
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowConfiguredFrontends", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

#endregion

#region Middleware Execution Pipeline

var app = builder.Build();

// Swagger configuration retained.
// Production exposure will be reviewed separately.
app.UseSwagger();
app.UseSwaggerUI();

// Preserve existing HTTPS behavior for Render compatibility.
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// Route requests before applying CORS.
app.UseRouting();

// Apply CORS before authentication and authorization.
app.UseCors("AllowConfiguredFrontends");

// Authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// REST controllers
app.MapControllers();

// Health endpoint for monitoring
app.MapHealthChecks("/health");

#endregion

#region Database Migrations

/*
 * Database migrations are executed only when explicitly enabled
 * through Database:ApplyMigrationsOnStartup.
 *
 * The default value is false when the property does not exist.
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

#endregion

// Start the application.
app.Run();
