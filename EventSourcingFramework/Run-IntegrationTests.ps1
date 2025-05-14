Write-Host "`nRunning integration tests locally..." -ForegroundColor Cyan

docker-compose up -d mongodb
try {
    dotnet test .\tests\Integration\EventSourcingFramework.Application.Test.Integration\
    if ($LASTEXITCODE -ne 0) { throw "Infrastructure tests failed." }
    dotnet test .\tests\Integration\EventSourcingFramework.Infrastructure.Test.Integration\
    if ($LASTEXITCODE -ne 0) { throw "Application tests failed." }
    Write-Host "`nAll local tests passed." -ForegroundColor Green
}
catch {
    Write-Host "`nTest run failed: $_" -ForegroundColor Red
    exit 1
}
finally {
    docker-compose down -v
}
