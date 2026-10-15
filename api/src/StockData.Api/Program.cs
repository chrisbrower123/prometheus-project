using StockData.Api.ErrorHandling;
using StockData.Domain.DependencyInjection;
using StockData.Infrastructure.Providers.Yahoo;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<StockDataExceptionHandler>();
builder.Services.AddStockDataDomain();
builder.Services.AddYahooMarketData(builder.Configuration);
builder.Services.AddHealthChecks();
if (builder.Environment.IsDevelopment()) { builder.Services.AddEndpointsApiExplorer(); builder.Services.AddSwaggerGen(); }
var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
// TODO: CORS policy for the frontend dev origin.
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
public partial class Program { }
