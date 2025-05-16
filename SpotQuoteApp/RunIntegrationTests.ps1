Write-Host "`nRunning integration tests locally..." -ForegroundColor Cyan

docker compose -f ../docker-compose.spotquoteapp.dev.test.yml up -d --build

try {
    dotnet test .\tests\SpotQuoteApp.Application.Test.Integration\ --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "Integration tests failed." }
    Write-Host "`nAll local tests passed." -ForegroundColor Green
}
catch {
    Write-Host "`nTest run failed: $_" -ForegroundColor Red
    exit 1
}
finally {
     docker compose -f ../docker-compose.spotquoteapp.dev.test.yml down -v
}
