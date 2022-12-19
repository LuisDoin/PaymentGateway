using MassTransit;
using Model.ModelValidationServices;
using PaymentProcessor.Config;
using PaymentProcessor.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<IPaymentValidationService, PaymentValidationService>();

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<PaymentConsumer>();

    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration.GetSection("RabbitMQ").GetSection("Uri").Value);

        cfg.ReceiveEndpoint(builder.Configuration.GetSection("RabbitMQ").GetSection("pendingTransactionsQueue").Value, c =>
        {
            c.ConfigureConsumer<PaymentConsumer>(ctx);
            c.UseMessageRetry(r => r.Immediate(5));
        });
    });
});

builder.Services.AddHttpClient();
builder.Services.AddControllers();

builder.Services.Configure<CKOBankSettings>(builder.Configuration.GetSection("CKOBank"));
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run();
