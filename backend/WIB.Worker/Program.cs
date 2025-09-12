using WIB.Application.Interfaces;
using WIB.Application.Receipts;
using WIB.Infrastructure.Clients;
using WIB.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddScoped<IOcrClient, OcrClient>();
builder.Services.AddScoped<IKieClient, KieClient>();
builder.Services.AddScoped<IProductClassifier, ProductClassifier>();
builder.Services.AddScoped<IReceiptStorage, ReceiptStorage>();
builder.Services.AddScoped<ProcessReceiptCommandHandler>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
