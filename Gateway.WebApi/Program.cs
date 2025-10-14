using System.Reflection;
using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using CacheService;
using DataBaseToAccess;
using DataBaseToAccess.Repositiory;
using DataBaseToAccess.Repositiory.RepositoryEntity;
using DotNetEnv;
using Gateway.WebApi.BackgroundService;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Service.Application.Iterfaces;
using Service.Application.Service.AddOnsQuery;
using Service.Application.Service.CartQuery;
using Service.Application.Service.FavoriteQuery;
using Service.Application.Service.GamesQuery;
using Service.Application.Service.GetNewsList;
using Service.Application.Service.OrderQuery;
using Service.Application.Service.ProductQuery;
using Service.Application.Service.SubscriptionsQuery;
using Service.Application.Service.TransactionQuery;
using Service.Application.Service.UserQuery;
using Services.CalculationService;
using Services.GetRegionFromCookie;
using Services.ParseService;
using Services.Payment;
using StackExchange.Redis;

// Загружаем переменные из .env файла ПЕРЕД созданием builder
Env.Load();
var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddSingleton<IRedisRepository, RedisRepository>();
builder.Services.AddTransient(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient(typeof(IGameRepository<>), typeof(GameRepository<>));
builder.Services.AddTransient(typeof(IProductRepository<>), typeof(ProductRepository<>));
builder.Services.AddTransient(typeof(IEditionRepository<>), typeof(EditionRepository<>));
builder.Services.AddTransient(typeof(ISubscriptionRepository<>), typeof(SubscriptionRepository<>));
builder.Services.AddTransient(typeof(IUserRepository<>), typeof(UserRepository<>));
builder.Services.AddTransient(typeof(IGenersRepository<>), typeof(GenersRepository<>));

builder.Services.AddScoped<ICalculationService, CalculatePrice>();
builder.Services.AddScoped<IDataFromCookie, DataFromCookie>();
builder.Services.AddScoped<ICacheService, ExchangeRate>();
builder.Services.AddScoped<IAuthService, Services.Autarization.Auth>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddScoped<FavoriteQuery>();
builder.Services.AddScoped<CartQuery>();
builder.Services.AddScoped<ProductQuery>();
builder.Services.AddScoped<GamesQuery>();
builder.Services.AddScoped<Parse>();
builder.Services.AddScoped<UsersQuery>();
builder.Services.AddScoped<NewsQuery>();
builder.Services.AddScoped<SubscriptionsQuerys>();
builder.Services.AddScoped<OrderQuery>();
builder.Services.AddScoped<AddOnsQuery>();
builder.Services.AddScoped<TransactionQuery>();
builder.Services.AddQuartz(q =>
{
    // Конфигурация Quartz
    q.UseMicrosoftDependencyInjectionJobFactory();
    q.UseSimpleTypeLoader();
    q.UseInMemoryStore();
    q.UseDefaultThreadPool(tp => tp.MaxConcurrency = 10);

    // // Создаем задание для ParseGames
    // var parseGamesJobKey = new JobKey("ParseGamesJob");
    // q.AddJob<ParseGamesJob>(opts => opts.WithIdentity(parseGamesJobKey));

    // // Создаем триггер для ParseGames (каждый день в 4:00)
    // q.AddTrigger(opts =>
    //     opts.ForJob(parseGamesJobKey)
    //         .WithIdentity("ParseGamesJob-trigger")
    //         .WithCronSchedule("0 20 3 * * ?") // секунды минуты часы день месяц день_недели
    // );

    // // Создаем задание для UpdateProductsPrice
    // var updatePriceJobKey = new JobKey("UpdateProductsPriceJob");
    // q.AddJob<UpdateProductsPriceJob>(opts => opts.WithIdentity(updatePriceJobKey));

    // // Создаем триггер для UpdateProductsPrice (каждый день в 4:05)
    // q.AddTrigger(opts =>
    //     opts.ForJob(updatePriceJobKey)
    //         .WithIdentity("UpdateProductsPriceJob-trigger")
    //         .WithCronSchedule("0 35 3 * * ?") // секунды минуты часы день месяц день_недели
    // );
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

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
});

//To Do: ��������� Cors
// builder.Services.AddCors(options =>
//     options.AddPolicy(
//         "AllowAll",
//         policy =>
//         {
//             policy.WithOrigins("http://gateway:8080");
//             policy.AllowAnyMethod();
//             policy.AllowAnyOrigin();
//         }
//     )
// );

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<BaseDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "������ ��� ���������� �������� � ���� ������.");
    }
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

//ToDo: �������� Middleware ��� ��������� ������
app.UseRouting();

app.UseHttpsRedirection();

// app.UseCors("AllowAll");

app.MapControllers();

app.Run();
