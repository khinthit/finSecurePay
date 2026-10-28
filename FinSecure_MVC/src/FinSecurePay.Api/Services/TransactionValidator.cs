using System.Text.RegularExpressions;
using FinSecurePay.Api.Models;

namespace FinSecurePay.Api.Services;

/// <summary>
/// Applies the six FinSecure Pay validation rules and returns every reason a
/// transaction fails, not just the first one.
/// </summary>
public class TransactionValidator
{
    private static readonly Regex AccountPattern = new("^ACC[0-9]{6}$", RegexOptions.Compiled);

    private static readonly HashSet<string> BlockedMerchants =
        new(StringComparer.OrdinalIgnoreCase) { "GAMBLING", "CRYPTO_ATM" };

    private readonly decimal _maxAmount;
    private readonly HashSet<string> _allowedCurrencies;

    public TransactionValidator(decimal maxAmount, IEnumerable<string> allowedCurrencies)
    {
        _maxAmount = maxAmount;
        _allowedCurrencies = new HashSet<string>(allowedCurrencies, StringComparer.OrdinalIgnoreCase);
    }

    public ValidationResult Validate(TransactionRequest tx)
    {
        var reasons = new List<string>();

        // Rules 1 and 2: required fields, then account format
        if (string.IsNullOrWhiteSpace(tx.TransactionId) || string.IsNullOrWhiteSpace(tx.AccountId))
            reasons.Add("MISSING_FIELDS");
        else if (!AccountPattern.IsMatch(tx.AccountId))
            reasons.Add("INVALID_ACCOUNT_FORMAT");

        // Rules 3 and 4: positive amount, within the configured limit
        if (tx.Amount is null || tx.Amount <= 0)
            reasons.Add("NON_POSITIVE_AMOUNT");
        else if (tx.Amount > _maxAmount)
            reasons.Add("AMOUNT_OVER_LIMIT");

        // Rule 5: supported currency
        if (string.IsNullOrWhiteSpace(tx.Currency) || !_allowedCurrencies.Contains(tx.Currency))
            reasons.Add("UNSUPPORTED_CURRENCY");

        // Rule 6: merchant category not blocked
        if (!string.IsNullOrWhiteSpace(tx.MerchantCategory) && BlockedMerchants.Contains(tx.MerchantCategory))
            reasons.Add("BLOCKED_MERCHANT");

        return new ValidationResult(tx.TransactionId, reasons.Count == 0, reasons);
    }
}
