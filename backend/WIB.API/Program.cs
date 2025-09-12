using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WIB.Application.Interfaces;
using WIB.Application.Receipts;
using WIB.Infrastructure;
using WIB.Infrastructure.Clients;
using WIB.Infrastructure.Options;

using WIB.API.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PostgresOptions>(builder.Configuration.GetSection("Postgres"));
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));
builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection("Minio"));
builder.Services.Configure<OcrOptions>(builder.Configuration.GetSection("Ocr"));
builder.Services.Configure<MlOptions>(builder.Configuration.GetSection("Ml"));
builder.Services.Configure<QdrantOptions>(builder.Configuration.GetSection("Qdrant"));

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetSection("Postgres").GetValue<string>("Connection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddHttpClient<MlController>((sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<MlOptions>>().Value;
    client.BaseAddress = new Uri(opts.Endpoint);
});

builder.Services.AddScoped<IOcrClient, OcrClient>();
builder.Services.AddScoped<IKieClient, KieClient>();
builder.Services.AddScoped<IProductClassifier, ProductClassifier>();
builder.Services.AddScoped<IReceiptStorage, ReceiptStorage>();
builder.Services.AddScoped<ProcessReceiptCommandHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/healthz");

app.Run();
