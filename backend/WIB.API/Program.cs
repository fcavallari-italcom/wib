using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WIB.Application.Interfaces;
using WIB.Application.Receipts;
using WIB.Infrastructure;
using WIB.Infrastructure.Clients;
using WIB.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PostgresOptions>(builder.Configuration.GetSection("Postgres"));
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));
builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection("Minio"));
builder.Services.Configure<OcrOptions>(builder.Configuration.GetSection("Ocr"));
builder.Services.Configure<MlOptions>(builder.Configuration.GetSection("Ml"));
builder.Services.Configure<QdrantOptions>(builder.Configuration.GetSection("Qdrant"));

builder.Services.AddDbContext<WibDbContext>((sp, opts) =>
{
    var pg = sp.GetRequiredService<IOptions<PostgresOptions>>().Value;
    opts.UseNpgsql(pg.ConnectionString);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddHttpClient("ml", (sp, client) =>
{
    var ml = sp.GetRequiredService<IOptions<MlOptions>>().Value;
    client.BaseAddress = new Uri(ml.Endpoint);
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

app.MapHealthChecks("/healthz");
app.MapControllers();

app.Run();
