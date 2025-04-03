using System.Reflection;
using Business.Data.Iterfaces.Store;
using Business.Data.Iterfaces;
using DataBaseToAccess;
using DataBaseToAccess.Repositiory;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Service.Application.Service.TransactionQuery;
using Service.Application.Service.SectionQuery;
using DataBaseToAccess.Repositiory.RepositoryEntity;
using Service.Application.Service.GetNewsList;
using Service.Application.Service.MarkUpQuery;
using Service.Application.Service.SubscriptionsQuery;
using Service.Application.Iterfaces;
using Services.CalculationService;
using Services.GetRegionFromCookie;
using CacheService;
using Service.Application.Service.UserQuery;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<BaseDbContext>(options =>
   options.UseNpgsql(builder.Configuration.GetConnectionString("DataBaseConnection")));


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

builder.Services.AddScoped<ICalculationService, CalculatePrice>();
builder.Services.AddScoped<IDataFromCookie, DataFromCookie>();
builder.Services.AddScoped<ICacheService, ExchangeRate>();

builder.Services.AddControllers();
builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Services.AddSwaggerGen(config =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    config.IncludeXmlComments(xmlPath);
}
);
//To Do: Настроить Cors 
builder.Services.AddCors(options =>
options.AddPolicy("AllowAll", policy =>
{
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
    policy.AllowAnyOrigin();

}));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

//ToDo: Напимать Middleware для обработки ошибок 
app.UseRouting();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapControllers();


app.Run();
