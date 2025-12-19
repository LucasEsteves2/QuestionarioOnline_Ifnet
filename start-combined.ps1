# Script para rodar API + Workers no mesmo terminal (usando Jobs)
# Uso: .\start-combined.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Iniciando API + Workers + RabbitMQ" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Iniciar RabbitMQ
Write-Host "Iniciando RabbitMQ..." -ForegroundColor Yellow
docker-compose up -d
Start-Sleep -Seconds 15

# 2. Iniciar API em background
Write-Host "Iniciando API..." -ForegroundColor Yellow
$apiJob = Start-Job -ScriptBlock {
    Set-Location "C:\Users\Lucas Dev\source\repos\QuestionarioOnline"
    dotnet run --project QuestionarioOnline\QuestionarioOnline.Api.csproj
}

Start-Sleep -Seconds 5

# 3. Iniciar Workers em background
Write-Host "Iniciando Workers..." -ForegroundColor Yellow
$workersJob = Start-Job -ScriptBlock {
    Set-Location "C:\Users\Lucas Dev\source\repos\QuestionarioOnline"
    dotnet run --project QuestionarioOnline.Workers.Function\QuestionarioOnline.Workers.Function.csproj
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Tudo rodando!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "URLs:" -ForegroundColor Cyan
Write-Host "  - Swagger: https://localhost:7001/swagger" -ForegroundColor White
Write-Host "  - RabbitMQ: http://localhost:15672" -ForegroundColor White
Write-Host ""
Write-Host "Pressione Ctrl+C para parar todos os serviços" -ForegroundColor Yellow
Write-Host ""

# Monitorar logs dos jobs
try {
    while ($true) {
        # Mostrar output da API
        $apiOutput = Receive-Job -Job $apiJob -ErrorAction SilentlyContinue
        if ($apiOutput) {
            Write-Host "[API] $apiOutput" -ForegroundColor Green
        }

        # Mostrar output dos Workers
        $workersOutput = Receive-Job -Job $workersJob -ErrorAction SilentlyContinue
        if ($workersOutput) {
            Write-Host "[WORKERS] $workersOutput" -ForegroundColor Cyan
        }

        Start-Sleep -Milliseconds 500
    }
}
finally {
    Write-Host ""
    Write-Host "Parando serviços..." -ForegroundColor Yellow
    
    Stop-Job -Job $apiJob -ErrorAction SilentlyContinue
    Stop-Job -Job $workersJob -ErrorAction SilentlyContinue
    
    Remove-Job -Job $apiJob -ErrorAction SilentlyContinue
    Remove-Job -Job $workersJob -ErrorAction SilentlyContinue
    
    Write-Host "? Serviços parados" -ForegroundColor Green
}
