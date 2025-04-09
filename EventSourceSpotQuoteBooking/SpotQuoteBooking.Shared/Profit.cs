namespace SpotQuoteBooking.Shared;

public class Profit
{
    public double Value { get; set; }
    public bool IsPercentage { get; set; }

    public double CalculatedProfit(double price) => IsPercentage ? price * Value * 0.01 : Value;
}
