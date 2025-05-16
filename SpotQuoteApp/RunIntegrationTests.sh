#!/bin/bash

echo -e "\nRunning integration tests locally..."

docker compose -f ../docker-compose.spotquoteapp.dev.test.yml up -d --build

function cleanup {
    echo -e "\nCleaning up Docker containers..."
    docker compose -f ../docker-compose.spotquoteapp.dev.test.yml down -v
}
trap cleanup EXIT

dotnet test ./tests/SpotQuoteApp.Application.Test.Integration/ --verbosity minimal
if [ $? -ne 0 ]; then
    echo -e "\Integration tests failed." >&2
    exit 1
fi

echo -e "\nAll local tests passed."
