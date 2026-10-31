using FinSecurePay.Api.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// One line per log entry, so kubectl logs and CloudWatch are easy to search.
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o =>
{
    o.SingleLine = true;
    o.TimestampFormat = "yyyy-MM-ddTHH:mm:ss ";
});

// Validation settings come from appsettings.json, overridden in Kubernetes by
// environment variables such as Validation__MaxAmount (from the ConfigMap).
builder.Services.Configure<ValidationOptions>(builder.Configuration.GetSection("Validation"));
builder.Services.AddSingleton(sp =>
{
    var o = sp.GetRequiredService<IOptions<ValidationOptions>>().Value;
    var currencies = o.AllowedCurrencies.Split(',',
        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    return new TransactionValidator(o.MaxAmount, currencies);
});

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseRouting();
app.UseHttpMetrics();                 // request count + latency metrics for Prometheus

app.MapControllers();

// Liveness: the process is up (no dependency checks).
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
// Readiness: all registered health checks pass.
app.MapHealthChecks("/health/ready");

app.MapMetrics();                     // GET /metrics

app.Run();

// Lets test projects reference the entry point if integration tests are added later.
public partial class Program { }
