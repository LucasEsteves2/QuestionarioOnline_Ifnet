# Script para iniciar API + Workers + RabbitMQ
# Uso: .\start-all.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  QuestionarioOnline - Startup Completo" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Verificar Docker
Write-Host "[1/3] Verificando Docker..." -ForegroundColor Yellow
try {
    docker ps | Out-Null
    Write-Host "  ? Docker está rodando" -ForegroundColor Green
} catch {
    Write-Host "  ? Docker não está rodando" -ForegroundColor Red
    Write-Host "  Inicie o Docker Desktop e tente novamente" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# 2. Iniciar RabbitMQ
Write-Host "[2/3] Iniciando RabbitMQ..." -ForegroundColor Yellow

$rabbitRunning = docker ps --filter "name=questionario-rabbitmq" --format "{{.Names}}"

if ($rabbitRunning -eq "questionario-rabbitmq") {
    Write-Host "  ? RabbitMQ já está rodando" -ForegroundColor Green
} else {
    docker-compose up -d
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ? RabbitMQ iniciado" -ForegroundColor Green
        Write-Host "  Aguardando RabbitMQ ficar pronto..." -ForegroundColor Yellow
        Start-Sleep -Seconds 15
    } else {
        Write-Host "  ? Falha ao iniciar RabbitMQ" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""

# 3. Iniciar API e Workers
Write-Host "[3/3] Iniciando API e Workers..." -ForegroundColor Yellow
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Tudo iniciado!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "URLs disponíveis:" -ForegroundColor Cyan
Write-Host "  - API:              https://localhost:7001" -ForegroundColor White
Write-Host "  - Swagger:          https://localhost:7001/swagger" -ForegroundColor White
Write-Host "  - RabbitMQ UI:      http://localhost:15672 (admin/admin123)" -ForegroundColor White
Write-Host ""
Write-Host "Os projetos serão iniciados em terminais separados..." -ForegroundColor Yellow
Write-Host "Pressione Ctrl+C em cada terminal para parar" -ForegroundColor Yellow
Write-Host ""

# Iniciar API em novo terminal
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "Write-Host '=== API ===' -ForegroundColor Green; dotnet run --project QuestionarioOnline\QuestionarioOnline.Api.csproj"
)

# Aguardar 2 segundos
Start-Sleep -Seconds 2

# Iniciar Workers em novo terminal
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "Write-Host '=== WORKERS ===' -ForegroundColor Cyan; dotnet run --project QuestionarioOnline.Workers.Function\QuestionarioOnline.Workers.Function.csproj"
)

Write-Host "? API e Workers foram iniciados em terminais separados" -ForegroundColor Green
Write-Host ""
