using FluentAssertions.Common;
using FluentMigrator.Runner;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ServiceIntegrationLibrary.Utils;
using ServiceIntegrationLibrary.Utils.Interfaces;
using System.Reflection;
using TransactionsApi.Config;
using TransactionsApi.Consumers;
using TransactionsApi.Context;
using TransactionsApi.Data;
using TransactionsApi.Data.Repositories;
using TransactionsApi.Extensions;
using TransactionsApi.Migrations;
using TransactionsApi.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging(c => c.AddFluentMigratorConsole())
        .AddFluentMigratorCore()
        .ConfigureRunner(c => c.AddSqlServer2012()
            .WithGlobalConnectionString(builder.Configuration.GetConnectionString("SqlConnection"))
            .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations());

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<SuccessfulTransactionsConsumer>();

    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration.GetSection("RabbitMQ").GetSection("Uri").Value);

        cfg.ReceiveEndpoint(builder.Configuration.GetSection("RabbitMQ").GetSection("CompletedTransactionsQueue").Value, c =>
        {
            c.ConfigureConsumer<SuccessfulTransactionsConsumer>(ctx);
            c.UseMessageRetry(r => r.Immediate(5));
        });
    });
});

// Add services to the container.
builder.Services.AddSingleton<Database>();
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddScoped<IPaymentServices, PaymentServices>();
builder.Services.AddScoped<IHttpClientProvider, HttpClientProvider>();
builder.Services.AddScoped<IPaymentsRepository, PaymentsRepository>();
builder.Services.AddScoped<DbSession>();

builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("PaymentGateway"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MigrateDatabase();

app.Run();
