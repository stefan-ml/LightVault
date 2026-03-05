using LightVault.Domain.Interfaces;
using LightVault.Infrastructure.Data;
using LightVault.Infrastructure.Seed;
using LightVault.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");
var jwtKey = builder.Configuration["JwtKey"];
var masterKeyBase64 = builder.Configuration["MasterKey"];
var auditSigningKey = builder.Configuration["AuditSigningKey"];

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Missing environment variable: ConnectionStrings__Default");

if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Missing environment variable: JwtKey");

if (string.IsNullOrWhiteSpace(masterKeyBase64))
    throw new InvalidOperationException("Missing environment variable: MasterKey");

if (string.IsNullOrWhiteSpace(auditSigningKey))
    throw new InvalidOperationException("Missing environment variable: AuditSigningKey");

builder.Services.AddDbContext<LightVaultDbContext>(options =>
    options.UseSqlServer(connectionString));

var masterKey = Convert.FromBase64String(masterKeyBase64);
builder.Services.AddSingleton<IEncryptionService>(
    new EncryptionService(masterKey)
);
builder.Services.AddScoped<IApiKeyHasher, Sha256ApiKeyHasher>();

builder.Services.AddControllers();

builder.Services.AddSingleton(new JwtService(builder.Configuration["JwtKey"]!));
builder.Services.AddScoped<AuditService>();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", opts =>
    {
        opts.TokenValidationParameters = new()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtKey"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LightVaultDbContext>();
    db.Database.Migrate();
    SeedData.EnsureSeeded(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("LightVault API")
            .WithTheme(ScalarTheme.Saturn)
            .AddDocument("v1", routePattern: "/swagger/{documentName}/swagger.json");
    });
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
