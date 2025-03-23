using System.Reflection;
using DataBaseToAccess;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<BaseDbContext>(options =>
   options.UseNpgsql(builder.Configuration.GetConnectionString("DataBaseConnection")));

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
