namespace EventSource.Core.Test;

public class Quote : IEntity
{
    public Quote(double price, string currency, string name)
    {
        Price = price;
        Currency = currency;
        Name = name;
    }

    public double Price { get; set; }
    public string Currency { get; set; }
    public string Name { get; set; }
    public Guid Id { get; } = Guid.NewGuid();
}
