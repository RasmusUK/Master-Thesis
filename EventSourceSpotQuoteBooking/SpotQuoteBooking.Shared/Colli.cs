namespace SpotQuoteBooking.Shared;

public class Colli
{
    public int NumberOfUnits { get; set; }
    public ColliType ColliType { get; set; }
    public double Length { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Weight { get; set; }
    public double Cbm => Length * Width * Height / 1000000;

    public Colli() { }

    public Colli(
        int NumberOfUnits,
        ColliType ColliType,
        double Length,
        double Width,
        double Height,
        double Weight
    )
    {
        this.NumberOfUnits = NumberOfUnits;
        this.ColliType = ColliType;
        this.Length = Length;
        this.Width = Width;
        this.Height = Height;
        this.Weight = Weight;
    }
}
