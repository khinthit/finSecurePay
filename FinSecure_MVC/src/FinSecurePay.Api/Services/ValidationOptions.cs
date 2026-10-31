namespace FinSecurePay.Api.Services;

public class ValidationOptions
{
    public decimal MaxAmount { get; set; } = 10000m;
    public string AllowedCurrencies { get; set; } = "SGD,USD,EUR";
}
