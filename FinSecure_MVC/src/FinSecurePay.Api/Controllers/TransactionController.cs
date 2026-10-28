using FinSecurePay.Api.Models;
using FinSecurePay.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Prometheus;

namespace FinSecurePay.Api.Controllers;

[ApiController]
public class TransactionController : ControllerBase
{
    // Shown in Grafana as approved vs rejected per second.
    private static readonly Counter Validations = Metrics.CreateCounter(
        "finsecure_validations_total", "Transaction validations by result", "result");

    private readonly TransactionValidator _validator;
    private readonly ILogger<TransactionController> _logger;
    private readonly string _version;

    public TransactionController(TransactionValidator validator,
                                 ILogger<TransactionController> logger,
                                 IConfiguration config)
    {
        _validator = validator;
        _logger = logger;
        _version = config["APP_VERSION"] ?? "dev";
    }

    [HttpGet("/health")]
    public IActionResult Health() =>
        Ok(new { status = "UP", service = "finsecure-pay", version = _version });

    [HttpPost("/validate")]
    public IActionResult Validate([FromBody] TransactionRequest request)
    {
        var result = _validator.Validate(request);

        if (result.Valid)
        {
            Validations.WithLabels("approved").Inc();
            _logger.LogInformation("validation txn={Txn} result=APPROVED", result.TransactionId);
            return Ok(result);
        }

        Validations.WithLabels("rejected").Inc();
        _logger.LogWarning("validation txn={Txn} result=REJECTED reasons={Reasons}",
            result.TransactionId, string.Join(",", result.Reasons));
        return UnprocessableEntity(result);
    }
}
