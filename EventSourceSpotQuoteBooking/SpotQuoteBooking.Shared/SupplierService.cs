namespace SpotQuoteBooking.Shared;

public record SupplierService(string Name, ForwarderService ForwarderService)
{
    public static readonly SupplierService DHLJetline = new(
        "DHL SameDay Jetline",
        ForwarderService.DHLSameDay
    );
    public static readonly SupplierService DHLSprintline = new(
        "DHL SameDay Sprintline",
        ForwarderService.DHLSameDay
    );
    public static readonly SupplierService DHLSpeedline = new(
        "DHL SameDay Speedline",
        ForwarderService.DHLSameDay
    );
    public static readonly SupplierService DHLExpressWorldwide = new(
        "DHL Express Worldwide",
        ForwarderService.DHLExpress
    );
    public static readonly SupplierService DHLExpress12 = new(
        "DHL Express 12:00",
        ForwarderService.DHLExpress
    );
    public static readonly SupplierService DHLFreightClassic = new(
        "DHL Freight Classic",
        ForwarderService.DHLFreight
    );
    public static readonly SupplierService DHLFreightPremium = new(
        "DHL Freight Premium",
        ForwarderService.DHLFreight
    );
    public static readonly SupplierService FedExInternationalPriority = new(
        "FedEx International Priority",
        ForwarderService.FedExExpress
    );
    public static readonly SupplierService FedExFirstOvernight = new(
        "FedEx First Overnight",
        ForwarderService.FedExExpress
    );
    public static readonly SupplierService FedExHomeDelivery = new(
        "FedEx Home Delivery",
        ForwarderService.FedExGround
    );
    public static readonly SupplierService FedExSmartPost = new(
        "FedEx SmartPost",
        ForwarderService.FedExGround
    );
    public static readonly SupplierService UPSWorldwideSaver = new(
        "UPS Worldwide Saver",
        ForwarderService.UPSExpress
    );
    public static readonly SupplierService UPSNextDayAir = new(
        "UPS Next Day Air",
        ForwarderService.UPSExpress
    );
    public static readonly SupplierService UPSFreightLTL = new(
        "UPS Freight LTL",
        ForwarderService.UPSFreight
    );
    public static readonly SupplierService UPSFreightTruckload = new(
        "UPS Freight Truckload",
        ForwarderService.UPSFreight
    );
    public static readonly SupplierService AramexPriorityExpress = new(
        "Aramex Priority Express",
        ForwarderService.AramexExpress
    );
    public static readonly SupplierService AramexNextDay = new(
        "Aramex Next Day",
        ForwarderService.AramexExpress
    );
    public static readonly SupplierService MaerskSpot = new(
        "Maersk Spot",
        ForwarderService.MaerskOcean
    );
    public static readonly SupplierService MaerskDailyMaersk = new(
        "Maersk Daily Maersk",
        ForwarderService.MaerskOcean
    );
    public static readonly SupplierService DBSchenkerLandBasic = new(
        "DB Schenker Land Basic",
        ForwarderService.DBSchenkerLand
    );
    public static readonly SupplierService DBSchenkerLandPremium = new(
        "DB Schenker Land Premium",
        ForwarderService.DBSchenkerLand
    );

    public override string ToString() => Name;

    public static IReadOnlyCollection<SupplierService> GetByForwarderService(
        ForwarderService service
    ) => GetAll().Where(s => s.ForwarderService == service).ToList();

    public static IReadOnlyCollection<SupplierService> GetByTransportMode(TransportMode mode) =>
        GetAll().Where(s => s.ForwarderService.Mode == mode).ToList();

    public static IReadOnlyCollection<SupplierService> GetAll() =>
        typeof(SupplierService)
            .GetFields(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static
            )
            .Select(f => f.GetValue(null))
            .OfType<SupplierService>()
            .ToList();
}
