namespace SpotQuoteBooking.Shared.Dtos;

public class ColliDto
{
    public int NumberOfUnits { get; set; }
    public string ColliType { get; set; }
    public double Length { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Weight { get; set; }
    public double Cbm { get; set; }

    public Colli ToColli()
    {
        return new Colli(
            NumberOfUnits,
            Shared.ColliType.GetColliType(ColliType),
            Length,
            Width,
            Height,
            Weight,
            Cbm
        );
    }
}
