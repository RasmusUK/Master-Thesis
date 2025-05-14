const express = require('express');
const app = express();
const port = 3000;

app.use(express.json());

app.post('/api/buying-rates', (req, res) => {
  const {
    supplier,
    forwarderService,
    supplierService,
    countryFrom,
    countryTo,
    transportMode
  } = req.body;

  if (!supplier || !forwarderService || !supplierService || !countryFrom || !countryTo || !transportMode) {
    return res.status(400).json({
      error: 'Missing one or more required fields'
    });
  }

  const mockResult = {
    supplier,
    forwarderService,
    supplierService,
    countryFrom,
    countryTo,
    transportMode,
    price: (Math.random() * 1000 + 100).toFixed(2),
    validFrom: new Date().toISOString(),
    validTo: new Date(Date.now() + 30 * 86400000).toISOString(),
    chargeType: "Freight",
    costType: "Per Shipment"
  };

  res.json(mockResult);
});

app.listen(port, () => {
  console.log(`Mock Buying Rate API is running at http://localhost:${port}`);
});
