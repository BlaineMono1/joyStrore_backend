using System;
using DataBaseToAccess;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BaseDbContext>(options =>
   options.UseNpgsql(builder.Configuration.GetConnectionString("DataBaseConnection")));
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
