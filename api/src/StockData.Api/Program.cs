using StockData.Api.ErrorHandling;
using StockData.Domain.DependencyInjection;
using StockData.Infrastructure.Providers.Yahoo;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using StockData.Api;
using Microsoft.AspNetCore.Diagnostics;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddProblemDetails(o => o.CustomizeProblemDetails = ctx => ctx.ProblemDetails.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id ?? ctx.HttpContext.TraceIdentifier);
builder.Services.AddExceptionHandler<StockDataExceptionHandler>();
builder.Services.AddStockDataDomain();
builder.Services.AddYahooMarketData(builder.Configuration);
builder.Services.AddOptions<CorsSettings>().BindConfiguration(CorsSettings.SectionName);
builder.Services.AddOptions<CorsOptions>().Configure<IOptions<CorsSettings>>((options, settings) =>
{
    options.AddPolicy("Frontend", policy => policy.WithOrigins(settings.Value.AllowedOrigins).WithMethods("GET").AllowAnyHeader());
});
builder.Services.AddCors();
builder.Services.AddHealthChecks();
if (builder.Environment.IsDevelopment()) { builder.Services.AddEndpointsApiExplorer(); builder.Services.AddSwaggerGen(); }
var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages(async (StatusCodeContext context) =>
{
    var response = context.HttpContext.Response;
    if (response.HasStarted) return;
    var statusCode = response.StatusCode;
    response.Clear();
    response.StatusCode = statusCode;
    response.ContentType = "application/problem+json";
    await response.WriteAsJsonAsync(new { status = response.StatusCode, title = "Request failed", type = "about:blank", traceId = context.HttpContext.TraceIdentifier }, cancellationToken: context.HttpContext.RequestAborted);
});
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseRouting();
app.UseCors("Frontend");
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
public partial class Program { }
