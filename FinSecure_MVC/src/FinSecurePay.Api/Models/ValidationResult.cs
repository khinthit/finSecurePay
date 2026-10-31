namespace FinSecurePay.Api.Models;

public record ValidationResult(string? TransactionId, bool Valid, IReadOnlyList<string> Reasons);
