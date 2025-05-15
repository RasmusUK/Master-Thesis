const express = require('express');
const app = express();
const port = 3000;

app.use(express.json());

app.post('/api/buying-rates', (req, res) => {
  const {
    Supplier,
    ForwarderService,
    SupplierService,
    CountryFrom,
    CountryTo,
    TransportMode
    } = req.body;

  const chargeTypes = ['Freight', 'Fuel Surcharge', 'Storage'];
  const costTypes = ['Per Shipment', 'Per Kg', 'Per Cbm'];

  const Rates = chargeTypes.map((ChargeType, index) => ({
    Supplier,
    ForwarderService,
    SupplierService,
    CountryFrom,
    CountryTo,
    TransportMode,
    Price: Math.round((Math.random() * 1000 + 100) * 100) / 100,
    ValidFrom: new Date().toISOString(),
    ValidTo: new Date(Date.now() + 30 * 86400000).toISOString(),
    ChargeType,
    CostType: costTypes[index]
  }));

  res.status(200).json({ Rates });
});

app.listen(port, () => {
  console.log(`Mock Buying Rate API is running at http://localhost:${port}`);
});
