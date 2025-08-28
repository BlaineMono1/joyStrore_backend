using System.Reflection;
using System.Security.Claims;
using System.Text;
using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using CacheService;
using DataBaseToAccess;
using DataBaseToAccess.Repositiory;
using DataBaseToAccess.Repositiory.RepositoryEntity;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Service.Application.Iterfaces;
using Service.Application.Service.AdminsQuery;
using Service.Application.Service.AutahQuery;
using Service.Application.Service.GetNewsList;
using Service.Application.Service.MarkUpQuery;
using Service.Application.Service.OrderQuery;
using Service.Application.Service.SectionQuery;
using Service.Application.Service.SubscriptionsQuery;
using Service.Application.Service.TransactionQuery;
using Service.Application.Service.UserQuery;
using Services.CalculationService;
using Services.GetRegionFromCookie;
using Services.Payment;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

Env.Load(); // ��� ��� ������ � .env
builder.Configuration.AddEnvironmentVariables(); // ������������ �� ENV, ������� �� .env

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var key = Encoding.UTF8.GetBytes(builder.Configuration["JWT_KEY"]);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT_ISSUER"],
            ValidAudience = builder.Configuration["JWT_AUDIENCE"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            RoleClaimType = ClaimTypes.Role,
        };
    });

builder.Services.AddDbContext<BaseDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DataBaseConnection"))
);

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("[Resis] Connecting to Redis... ");
        var configuration = builder.Configuration.GetConnectionString("RedisConnection");
        return ConnectionMultiplexer.Connect(configuration);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "[Resis] Fail connection to Redis: {ErrorMessage}", ex.Message);
        throw;
    }
});
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IRedisRepository, RedisRepository>();

builder.Services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddTransient(typeof(IProductRepository<>), typeof(ProductRepository<>));
builder.Services.AddTransient(typeof(ISubscriptionRepository<>), typeof(SubscriptionRepository<>));
builder.Services.AddTransient(typeof(IUserRepository<>), typeof(UserRepository<>));

builder.Services.AddScoped<TransactionQuery>();
builder.Services.AddScoped<SectionQuery>();
builder.Services.AddScoped<NewsQuery>();
builder.Services.AddScoped<MarkUpQUery>();
builder.Services.AddScoped<SubscriptionsQuerys>();
builder.Services.AddScoped<UsersQuery>();
builder.Services.AddScoped<AdminsQuery>();
builder.Services.AddScoped<AutahQuery>();
builder.Services.AddScoped<OrderQuery>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ICalculationService, CalculatePrice>();
builder.Services.AddScoped<IDataFromCookie, DataFromCookie>();
builder.Services.AddScoped<ICacheService, ExchangeRate>();
builder.Services.AddScoped<IAuthService, Services.Autarization.Auth>();

builder.Services.AddControllers();
builder
    .Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft
            .Json
            .ReferenceLoopHandling
            .Ignore
    );

builder.Services.AddSwaggerGen(config =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    config.IncludeXmlComments(xmlPath);

    config.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Log in format: Bearer token",
        }
    );

    config.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );
});

//To Do: ��������� Cors
builder.Services.AddCors(options =>
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
            policy.AllowAnyOrigin();
        }
    )
);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

//ToDo: �������� Middleware ��� ��������� ������
app.UseRouting();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
