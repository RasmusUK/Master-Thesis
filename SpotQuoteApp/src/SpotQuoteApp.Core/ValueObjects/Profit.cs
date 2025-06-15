namespace SpotQuoteApp.Core.ValueObjects;

public record Profit(double Value, bool IsPercentage)
{
    public double CalculatedProfit(double price)
    {
        return IsPercentage ? price * Value * 0.01 : Value;
    }
}
