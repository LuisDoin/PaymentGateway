using CKOBankSimulator.Services;
using CKOBankSimulator.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<ICachingService, CachingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddStackExchangeRedisCache(o =>
{
    o.InstanceName = builder.Configuration.GetSection("Redis").GetSection("InstanceName").Value;
    o.Configuration = builder.Configuration.GetSection("Redis").GetSection("Uri").Value;
});

builder.Services.AddControllers();
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
