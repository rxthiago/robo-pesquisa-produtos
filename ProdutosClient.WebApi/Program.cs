using ProdutosClient.WebApi.Factories;
using ProdutosClient.WebApi.Services.ExportacaoPlanilha;
using ProdutosClient.WebApi.Services.FileImportService;
using ProdutosClient.WebApi.Services.ImageDownloadService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<SiteFactory>();
builder.Services.AddSingleton<CsvImportService>();
builder.Services.AddSingleton<ExportacaoPlanilhaService>();
builder.Services.AddSingleton<ImageDownloadService>();

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
