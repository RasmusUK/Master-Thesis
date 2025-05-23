#!/bin/bash

echo -e "\nRunning integration tests locally..."

docker compose up -d

function cleanup {
    echo -e "\nCleaning up Docker containers..."
    docker compose down -v
}
trap cleanup EXIT

dotnet test ./tests/Integration/EventSourcingFramework.Application.Test.Integration/ --verbosity minimal
if [ $? -ne 0 ]; then
    echo -e "\nInfrastructure tests failed." >&2
    exit 1
fi

dotnet test ./tests/Integration/EventSourcingFramework.Infrastructure.Test.Integration/ --verbosity minimal
if [ $? -ne 0 ]; then
    echo -e "\nApplication tests failed." >&2
    exit 1
fi

echo -e "\nAll local tests passed."
