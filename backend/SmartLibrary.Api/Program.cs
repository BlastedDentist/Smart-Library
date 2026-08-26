using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SmartLibrary.Api.Database;
using SmartLibrary.Api.Middleware;
using SmartLibrary.Api.Repositories;
using SmartLibrary.Api.Services;

// "Top-level statements": C# lets Program.cs run code directly at file scope
// instead of wrapping everything in `class Program { static void Main() }`.
// `var builder = WebApplication.CreateBuilder(args);` is doing what used to
// be many lines of boilerplate - it sets up configuration (appsettings.json,
// environment variables), logging, and the DI container in one call.
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);

// ----- 1. Configuration binding (Options pattern) -----
// Each of these maps a section of appsettings.json onto a strongly typed
// class, so the rest of the app injects IOptions<T> instead of reading
// magic strings out of IConfiguration everywhere.
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<AdminCredentials>(builder.Configuration.GetSection("AdminCredentials"));
builder.Services.Configure<QrSettings>(builder.Configuration.GetSection("QrSettings"));

// ----- 2. Dependency Injection registrations -----
// Singleton: one instance for the whole app's lifetime. Correct for
// MongoDbContext because MongoClient is thread-safe and expensive to create.
builder.Services.AddSingleton<MongoDbContext>();

// Scoped: one instance per HTTP request. This is the default/correct choice
// for repositories and services, since they don't hold state between requests.
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<ILibrarySettingsRepository, LibrarySettingsRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookLoanRepository, BookLoanRepository>();

builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<ILibraryService, LibraryService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBookLoanService, BookLoanService>();
builder.Services.AddScoped<IQrTokenService, QrTokenService>();

// ----- 3. ASP.NET Core services -----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Smart Library API",
        Version = "v1",
        Description = "API for the Smart Library Occupancy Monitoring and Analytics System"
    });

    // Lets Swagger UI accept a Bearer token so admin-only endpoints can be
    // tested directly from the /swagger page.
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Enter: Bearer {your JWT token}",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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

// ----- 4. CORS -----
// The React dev server runs on a different origin (localhost:5173) than the
// API (localhost:5000-ish), so the browser blocks requests unless we
// explicitly allow that origin. "AllowCredentials" isn't needed since we
// send the JWT as a header, not a cookie.
var corsOrigin = builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:5173";
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(corsOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ----- 5. Authentication (JWT Bearer) -----
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

// ----- 6. HTTP request pipeline -----
// Order matters here: exception handling wraps everything, then Swagger
// (dev only), then HTTPS redirect, then CORS, then auth, then controllers.
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Library API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
