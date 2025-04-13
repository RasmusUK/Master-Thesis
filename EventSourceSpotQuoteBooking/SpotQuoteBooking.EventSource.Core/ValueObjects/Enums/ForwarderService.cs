namespace SpotQuoteBooking.EventSource.Core.ValueObjects.Enums;

public record ForwarderService(string Name, Supplier Supplier, TransportMode Mode)
{
    public static readonly ForwarderService DHLSameDay = new(
        "DHL SameDay",
        Supplier.DHL,
        TransportMode.Air
    );
    public static readonly ForwarderService DHLExpress = new(
        "DHL Express",
        Supplier.DHL,
        TransportMode.Courier
    );
    public static readonly ForwarderService DHLFreight = new(
        "DHL Freight",
        Supplier.DHL,
        TransportMode.Road
    );
    public static readonly ForwarderService FedExExpress = new(
        "FedEx Express",
        Supplier.FedEx,
        TransportMode.Air
    );
    public static readonly ForwarderService FedExGround = new(
        "FedEx Ground",
        Supplier.FedEx,
        TransportMode.Road
    );
    public static readonly ForwarderService UPSExpress = new(
        "UPS Express",
        Supplier.UPS,
        TransportMode.Courier
    );
    public static readonly ForwarderService UPSFreight = new(
        "UPS Freight",
        Supplier.UPS,
        TransportMode.Road
    );
    public static readonly ForwarderService AramexExpress = new(
        "Aramex Express",
        Supplier.Aramex,
        TransportMode.Courier
    );
    public static readonly ForwarderService MaerskOcean = new(
        "Maersk Ocean",
        Supplier.Maersk,
        TransportMode.Sea
    );
    public static readonly ForwarderService DBSchenkerLand = new(
        "DB Schenker Land",
        Supplier.DB_Schenker,
        TransportMode.Road
    );

    public override string ToString() => Name;

    public static IReadOnlyCollection<ForwarderService> GetBySupplier(Supplier supplier) =>
        GetAll().Where(f => f.Supplier == supplier).OrderBy(s => s.Name).ToList();

    public static IReadOnlyCollection<ForwarderService> GetByTransportMode(TransportMode mode) =>
        GetAll().Where(f => f.Mode == mode).OrderBy(s => s.Name).ToList();

    public static IReadOnlyCollection<Supplier> GetSuppliersByTransportMode(TransportMode mode) =>
        GetByTransportMode(mode).Select(f => f.Supplier).Distinct().OrderBy(s => s.Value).ToList();

    public static IReadOnlyCollection<ForwarderService> GetAll() =>
        typeof(ForwarderService)
            .GetFields(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static
            )
            .Select(f => f.GetValue(null))
            .OfType<ForwarderService>()
            .ToList();
}
