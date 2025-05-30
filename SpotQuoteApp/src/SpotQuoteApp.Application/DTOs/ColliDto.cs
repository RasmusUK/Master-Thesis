using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Application.DTOs;

public class ColliDto
{
    public int NumberOfUnits { get; set; }
    public ColliType Type { get; set; }
    public double Length { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Weight { get; set; }
    public double Cbm { get; set; }
}
