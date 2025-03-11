using System.Reflection;
using DataBaseToAccess;
using Services.CalculationService;
using Services.GetRegionFromCookie;
using Microsoft.EntityFrameworkCore;
using Service.Application.Iterfaces;
using Business.Data.Iterfaces;
using DataBaseToAccess.Repositiory;
using Service.Application.Service.GamesQuery;
using Services.ParseService;
using Business.Data.Iterfaces.Store;
using DataBaseToAccess.Repositiory.RepositoryEntity;
using Service.Application.Service.UserQuery;
using Service.Application.Service.GetNewsList;
using Service.Application.Service.SubscriptionsQuery;
using StackExchange.Redis;
using CacheService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BaseDbContext>(options =>
   options.UseNpgsql(builder.Configuration.GetConnectionString("DataBaseConnection")));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("RedisConnection");
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddSingleton<IRedisRepository, RedisRepository>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient(typeof(IRepository<>), typeof(Repository<>));


builder.Services.AddTransient(typeof(IGameRepository<>), typeof(GameRepository<>));
builder.Services.AddTransient(typeof(IProductRepository<>), typeof(ProductRepository<>));
builder.Services.AddTransient(typeof(IEditionRepository<>), typeof(EditionRepository<>));
builder.Services.AddTransient(typeof(ISubscriptionRepository<>), typeof(SubscriptionRepository<>));
builder.Services.AddTransient(typeof(IUserRepository<>), typeof(UserRepository<>));
builder.Services.AddTransient(typeof(IGenersRepository<>), typeof(GenersRepository<>));

builder.Services.AddScoped<ICalculationService, CalculatePrice>();
builder.Services.AddScoped<IRegionFromCookie, RegionFromCookie>();
builder.Services.AddScoped<ICacheService, ExchangeRate>();


builder.Services.AddScoped<GamesQuery>();
builder.Services.AddScoped<Parse>();
builder.Services.AddScoped<UsersQuery>();
builder.Services.AddScoped<NewsQuery>();
builder.Services.AddScoped<SubscriptionsQuerys>();

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
