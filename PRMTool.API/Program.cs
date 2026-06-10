using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PRMTool.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "PRMTool API", Version = "v1" });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register Repositories
builder.Services.AddScoped<PRMTool.Domain.Interfaces.IUserRepository, PRMTool.Infrastructure.Repositories.UserRepository>();
builder.Services.AddScoped<PRMTool.Domain.Interfaces.IRoleRepository, PRMTool.Infrastructure.Repositories.RoleRepository>();
builder.Services.AddScoped<PRMTool.Domain.Interfaces.IEmployeeRepository, PRMTool.Infrastructure.Repositories.EmployeeRepository>();
builder.Services.AddScoped<PRMTool.Domain.Interfaces.IEmployeeSkillRepository, PRMTool.Infrastructure.Repositories.EmployeeSkillRepository>();
builder.Services.AddScoped<PRMTool.Domain.Interfaces.IProjectRepository, PRMTool.Infrastructure.Repositories.ProjectRepository>();
builder.Services.AddScoped<PRMTool.Domain.Interfaces.IMilestoneRepository, PRMTool.Infrastructure.Repositories.MilestoneRepository>();
builder.Services.AddScoped<PRMTool.Domain.Interfaces.IAllocationRepository, PRMTool.Infrastructure.Repositories.AllocationRepository>();
builder.Services.AddScoped<PRMTool.Domain.Interfaces.ISystemSettingRepository, PRMTool.Infrastructure.Repositories.SystemSettingRepository>();
builder.Services.AddScoped<PRMTool.Domain.Interfaces.ITimesheetRepository, PRMTool.Infrastructure.Repositories.TimesheetRepository>();

// Register Services
builder.Services.AddScoped<PRMTool.Application.Interfaces.IAuthService, PRMTool.Application.Services.AuthService>();
builder.Services.AddScoped<PRMTool.Application.Interfaces.IUserService, PRMTool.Application.Services.UserService>();
builder.Services.AddScoped<PRMTool.Application.Interfaces.IRoleService, PRMTool.Application.Services.RoleService>();
builder.Services.AddScoped<PRMTool.Application.Interfaces.IEmployeeService, PRMTool.Application.Services.EmployeeService>();
builder.Services.AddScoped<PRMTool.Application.Interfaces.IProjectService, PRMTool.Application.Services.ProjectService>();
builder.Services.AddScoped<PRMTool.Application.Interfaces.IAllocationService, PRMTool.Application.Services.AllocationService>();
builder.Services.AddScoped<PRMTool.Application.Interfaces.ISystemSettingsService, PRMTool.Application.Services.SystemSettingsService>();
builder.Services.AddScoped<PRMTool.Application.Interfaces.ITimesheetService, PRMTool.Application.Services.TimesheetService>();
builder.Services.AddScoped<PRMTool.Application.Interfaces.IManagerService, PRMTool.Application.Services.ManagerService>();
builder.Services.AddScoped<PRMTool.Application.Interfaces.IAIService, PRMTool.Application.Services.AIService>();

// Configure Authentication & JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing");
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // set to true in production
    options.SaveToken = true;
    // Map JWT claim names (e.g. "unique_name") back to ClaimTypes so User.Identity.Name works
    options.MapInboundClaims = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        NameClaimType = System.Security.Claims.ClaimTypes.Name,
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        builder => builder.WithOrigins("http://localhost:4200")
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PRMTool API v1"));
}

app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
