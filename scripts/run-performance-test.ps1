param(
    [int]$DurationSeconds = 60,
    [int]$ExtraPendingTasks = 10000,
    [int]$Port = 5081
)

$ErrorActionPreference = "Stop"
$taskRoot = Split-Path -Parent $PSScriptRoot
$taskTemp = [System.IO.Path]::GetTempPath()
$taskDatabase = Join-Path $taskTemp "sistema-tareas-perf-$([guid]::NewGuid().ToString('N')).db"
$taskOutput = Join-Path $taskTemp "sistema-tareas-perf-$([guid]::NewGuid().ToString('N')).log"
$taskErrorOutput = "$taskOutput.err"
$taskBaseUrl = "http://127.0.0.1:$Port"
$webProject = Join-Path $taskRoot "src\SistemaTareas.Web\SistemaTareas.Web.csproj"

$env:ASPNETCORE_ENVIRONMENT = "Testing"
$env:ConnectionStrings__Tareas = "Data Source=$taskDatabase"
$env:DemoData__ExtraPendingTasks = $ExtraPendingTasks.ToString()
$env:ASPNETCORE_URLS = $taskBaseUrl

$taskProcess = Start-Process dotnet `
    -ArgumentList @("run", "--project", "`"$webProject`"", "--no-build", "--no-restore", "--no-launch-profile") `
    -WorkingDirectory $taskRoot `
    -RedirectStandardOutput $taskOutput `
    -RedirectStandardError $taskErrorOutput `
    -WindowStyle Hidden `
    -PassThru

try {
    $taskReady = $false
    for ($attempt = 0; $attempt -lt 150; $attempt++) {
        try {
            $response = Invoke-WebRequest -UseBasicParsing "$taskBaseUrl/health" -TimeoutSec 1
            if ($response.StatusCode -eq 200) {
                $taskReady = $true
                break
            }
        }
        catch {
            Start-Sleep -Milliseconds 200
        }
    }

    if (-not $taskReady) {
        Get-Content $taskOutput -ErrorAction SilentlyContinue
        Get-Content $taskErrorOutput -ErrorAction SilentlyContinue
        throw "La aplicación no respondió al health check."
    }

    $env:SISTEMA_TAREAS_BASE_URL = $taskBaseUrl
    $env:PERFORMANCE_DURATION_SECONDS = $DurationSeconds.ToString()
    dotnet run `
        --project (Join-Path $taskRoot "tests\SistemaTareas.PerformanceTests\SistemaTareas.PerformanceTests.csproj") `
        --no-build `
        --no-restore

    if ($LASTEXITCODE -ne 0) {
        throw "La prueba de rendimiento no cumplió los umbrales."
    }
}
finally {
    if (-not $taskProcess.HasExited) {
        Stop-Process -Id $taskProcess.Id -Force
        $taskProcess.WaitForExit()
    }

    $taskFiles = @(
        $taskDatabase,
        "$taskDatabase-wal",
        "$taskDatabase-shm",
        $taskOutput,
        $taskErrorOutput
    )

    foreach ($taskFile in $taskFiles) {
        if ((Test-Path -LiteralPath $taskFile) -and $taskFile.StartsWith($taskTemp)) {
            Remove-Item -LiteralPath $taskFile -Force -ErrorAction SilentlyContinue
        }
    }
}
