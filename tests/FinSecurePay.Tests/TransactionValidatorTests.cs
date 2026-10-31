using FinSecurePay.Api.Models;
using FinSecurePay.Api.Services;

namespace FinSecurePay.Tests;

public class TransactionValidatorTests
{
    private readonly TransactionValidator _validator =
        new(10000m, new[] { "SGD", "USD", "EUR" });

    private static TransactionRequest Tx(string? acc, decimal? amount, string? ccy, string? merchant) =>
        new("TXN-1", acc, amount, ccy, merchant);

    [Fact]
    public void ValidTransaction_IsApproved()
    {
        var r = _validator.Validate(Tx("ACC123456", 250m, "SGD", "RETAIL"));
        Assert.True(r.Valid);
        Assert.Empty(r.Reasons);
    }

    [Fact]
    public void MissingAccount_IsRejected() =>
        Assert.Contains("MISSING_FIELDS", _validator.Validate(Tx(null, 10m, "SGD", "RETAIL")).Reasons);

    [Fact]
    public void BadAccountFormat_IsRejected() =>
        Assert.Contains("INVALID_ACCOUNT_FORMAT", _validator.Validate(Tx("12345", 10m, "SGD", "RETAIL")).Reasons);

    [Theory]
    [InlineData(0.0)]
    [InlineData(-5.0)]
    public void ZeroOrNegativeAmount_IsRejected(double amount) =>
        Assert.Contains("NON_POSITIVE_AMOUNT",
            _validator.Validate(Tx("ACC123456", (decimal)amount, "SGD", "RETAIL")).Reasons);

    [Fact]
    public void AmountOverLimit_IsRejected() =>
        Assert.Contains("AMOUNT_OVER_LIMIT", _validator.Validate(Tx("ACC123456", 10000.01m, "SGD", "RETAIL")).Reasons);

    [Fact]
    public void AmountAtLimit_IsApproved() =>
        Assert.True(_validator.Validate(Tx("ACC123456", 10000m, "USD", "RETAIL")).Valid);

    [Fact]
    public void UnsupportedCurrency_IsRejected() =>
        Assert.Contains("UNSUPPORTED_CURRENCY", _validator.Validate(Tx("ACC123456", 10m, "JPY", "RETAIL")).Reasons);

    [Fact]
    public void BlockedMerchant_IsRejected() =>
        Assert.Contains("BLOCKED_MERCHANT", _validator.Validate(Tx("ACC123456", 10m, "SGD", "gambling")).Reasons);

    [Fact]
    public void MultipleFailures_AreAllReported()
    {
        var r = _validator.Validate(Tx("BAD", 0m, "XYZ", "GAMBLING"));
        Assert.False(r.Valid);
        Assert.Equal(5, r.Reasons.Count);   // change 4 to 5 to demo a failing pipeline (Task 4)
    }
}
