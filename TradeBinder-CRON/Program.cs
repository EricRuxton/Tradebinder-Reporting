using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using TradeBinder_CRON;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<DailyPriceData>();
builder.Services.AddHostedService<PricingService>();
builder.Services.AddMySqlDataSource(builder.Configuration.GetConnectionString("Default")!);


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
