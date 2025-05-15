Write-Host "`nRunning integration tests locally..." -ForegroundColor Cyan

docker-compose up -d
try {
    dotnet test .\tests\Integration\EventSourcingFramework.Application.Test.Integration\ --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "Infrastructure tests failed." }
    dotnet test .\tests\Integration\EventSourcingFramework.Infrastructure.Test.Integration\ --verbosity minimal
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
