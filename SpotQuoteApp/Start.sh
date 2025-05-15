#!/bin/bash

docker compose -f ../docker-compose.yml up -d --build

echo "Spot Quote App is running at http://localhost:8080"