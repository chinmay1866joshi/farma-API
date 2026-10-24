using System.Text;
using FarmaApi.Data;
using FarmaApi.Models;
using FarmaApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers
builder.Services.AddControllers();

// 2. Configure PostgreSQL DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        options.UseNpgsql(connectionString);
    }
});

// 3. Register Custom Services
builder.Services.AddScoped<ITokenService, TokenService>();

// 4. Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "Kafh_Organic_Farm_Secure_Key_2026_Development_Secret_Key!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "FarmaApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "FarmaApiUsers";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 5. Configure CORS for Angular Frontend
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200", "https://localhost:4200", "http://127.0.0.1:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 6. Configure Swagger with JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Kafh Organic Farming REST API",
        Version = "v1",
        Description = "RESTful Web API for Kafh Organic Farm platform built with ASP.NET Core & PostgreSQL."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token format: Bearer {your token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

var app = builder.Build();

// 7. Configure HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kafh Farma API v1");
    c.RoutePrefix = "swagger"; // available at /swagger
});

// Root welcome / health endpoint
app.MapGet("/", () => Results.Ok(new
{
    application = "Kafh Farma API",
    version = "1.0.0",
    status = "Online",
    database = "PostgreSQL (Npgsql)",
    documentation = "/swagger",
    endpoints = new
    {
        auth_signup = "POST /api/auth/signup",
        auth_login = "POST /api/auth/login",
        auth_me = "GET /api/auth/me",
        products = "GET /api/products",
        orders = "GET /api/orders",
        farms = "GET /api/farms"
    }
}));

app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Attempt database connection check on startup
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        var canConnect = dbContext.Database.CanConnect();
        if (canConnect)
        {
            logger.LogInformation("Connected successfully to PostgreSQL database.");
            
            // Ensure FarmDocuments table exists in PostgreSQL
            dbContext.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""FarmDocuments"" (
                    ""Id"" uuid NOT NULL,
                    ""FarmerId"" uuid NOT NULL,
                    ""Title"" character varying(200) NOT NULL,
                    ""Category"" character varying(50) NOT NULL,
                    ""DocumentNumber"" character varying(100) NOT NULL,
                    ""IssuingAuthority"" character varying(200) NOT NULL,
                    ""IssueDate"" character varying(50) NOT NULL,
                    ""ExpiryDate"" character varying(50),
                    ""LandAreaAcres"" numeric(18,2),
                    ""PlotNumber"" character varying(100),
                    ""Status"" character varying(50) NOT NULL,
                    ""FileName"" character varying(255) NOT NULL,
                    ""FileSize"" character varying(50) NOT NULL,
                    ""FileType"" character varying(100) NOT NULL,
                    ""FileData"" text,
                    ""Notes"" character varying(1000),
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_FarmDocuments"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_FarmDocuments_Users_FarmerId"" FOREIGN KEY (""FarmerId"") REFERENCES ""Users"" (""Id"") ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS ""IX_FarmDocuments_FarmerId"" ON ""FarmDocuments"" (""FarmerId"");
            ");

            // Seed initial FarmDocuments in database if table is empty
            if (!dbContext.FarmDocuments.Any())
            {
                var farmer = dbContext.Users.FirstOrDefault(u => u.Role == "farmer") 
                             ?? dbContext.Users.FirstOrDefault();

                if (farmer != null)
                {
                    dbContext.FarmDocuments.AddRange(
                        new FarmDocument
                        {
                            Id = Guid.Parse("d1111111-1111-1111-1111-111111111111"),
                            FarmerId = farmer.Id,
                            Title = "7/12 Land Record Extract (Saat-Baara)",
                            Category = "land_record",
                            DocumentNumber = "MH-REV-712-90412",
                            IssuingAuthority = "Revenue & Land Records Dept, Maharashtra",
                            IssueDate = "2024-04-10",
                            LandAreaAcres = 14.50m,
                            PlotNumber = "Survey Plot 45/2B",
                            Status = "verified",
                            FileName = "7_12_land_extract_pune_dist.pdf",
                            FileSize = "1.8 MB",
                            FileType = "application/pdf",
                            Notes = "Certified title under organic farming cluster initiative.",
                            CreatedAt = DateTime.UtcNow.AddDays(-15)
                        },
                        new FarmDocument
                        {
                            Id = Guid.Parse("d2222222-2222-2222-2222-222222222222"),
                            FarmerId = farmer.Id,
                            Title = "NPOP National Organic Certification",
                            Category = "organic_cert",
                            DocumentNumber = "NPOP/NAB/0019/2024",
                            IssuingAuthority = "APEDA / National Programme for Organic Production",
                            IssueDate = "2024-01-15",
                            ExpiryDate = "2027-01-14",
                            LandAreaAcres = 14.50m,
                            PlotNumber = "Plots 45/2B & 46/1A",
                            Status = "verified",
                            FileName = "npop_organic_standard_certificate.pdf",
                            FileSize = "3.2 MB",
                            FileType = "application/pdf",
                            Notes = "100% chemical & pesticide free cultivation certified.",
                            CreatedAt = DateTime.UtcNow.AddDays(-10)
                        },
                        new FarmDocument
                        {
                            Id = Guid.Parse("d3333333-3333-3333-3333-333333333333"),
                            FarmerId = farmer.Id,
                            Title = "ICAR Soil Health Card & Nutrient Analysis",
                            Category = "soil_water",
                            DocumentNumber = "SHC-MH-2025-6671",
                            IssuingAuthority = "ICAR Soil & Agronomy Testing Laboratory",
                            IssueDate = "2025-06-20",
                            ExpiryDate = "2027-06-19",
                            LandAreaAcres = 14.50m,
                            PlotNumber = "Survey Plot 45/2B",
                            Status = "verified",
                            FileName = "soil_health_nutrient_report_2025.pdf",
                            FileSize = "2.1 MB",
                            FileType = "application/pdf",
                            Notes = "High organic carbon content (0.82%), balanced nutrients.",
                            CreatedAt = DateTime.UtcNow.AddDays(-5)
                        }
                    );
                    dbContext.SaveChanges();
                    logger.LogInformation("Seeded initial verified FarmDocuments in PostgreSQL database.");
                }
            }
        }
        else
        {
            logger.LogWarning("PostgreSQL database is currently unreachable. Please make sure PostgreSQL is running and connection string in appsettings.json is correct.");
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not connect to PostgreSQL on startup. Ensure PostgreSQL service is started.");
    }
}

app.Run();
