namespace FinSecurePay.Api.Models;

public record TransactionRequest(
    string? TransactionId,
    string? AccountId,
    decimal? Amount,
    string? Currency,
    string? MerchantCategory);
