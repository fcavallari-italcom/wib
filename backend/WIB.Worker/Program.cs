using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WIB.Application.Interfaces;
using WIB.Application.Receipts;
using WIB.Infrastructure;
using WIB.Infrastructure.Clients;
using WIB.Infrastructure.Options;
using WIB.Worker;

var builder = Host.CreateApplicationBuilder(args);
var cfg = builder.Configuration;

builder.Services.Configure<PostgresOptions>(cfg.GetSection("Postgres"));
builder.Services.Configure<RedisOptions>(cfg.GetSection("Redis"));
builder.Services.Configure<MinioOptions>(cfg.GetSection("Minio"));
builder.Services.Configure<OcrOptions>(cfg.GetSection("Ocr"));
builder.Services.Configure<MlOptions>(cfg.GetSection("Ml"));
builder.Services.Configure<QdrantOptions>(cfg.GetSection("Qdrant"));

builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(cfg.GetSection("Postgres").GetValue<string>("Connection")));

builder.Services.AddScoped<IOcrClient, OcrClient>();
builder.Services.AddScoped<IKieClient, KieClient>();
builder.Services.AddScoped<IProductClassifier, ProductClassifier>();
builder.Services.AddScoped<IReceiptStorage, ReceiptStorage>();
builder.Services.AddScoped<ProcessReceiptCommandHandler>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
