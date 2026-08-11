param(
    [int]$PerformanceDurationSeconds = 60,
    [int]$PerformanceExtraPendingTasks = 10000,
    [int]$PerformancePort = 5081,
    [switch]$SkipPerformance,
    [switch]$SkipSystem
)

$ErrorActionPreference = "Stop"
$taskRoot = Split-Path -Parent $PSScriptRoot
$resultsDirectory = Join-Path $taskRoot "artifacts\test-results"
$coverageDirectory = Join-Path $taskRoot "artifacts\coverage"
$results = [System.Collections.Generic.List[object]]::new()
New-Item -ItemType Directory -Force -Path $resultsDirectory, $coverageDirectory | Out-Null

function Invoke-Stage([string]$Name, [string]$LogName, [scriptblock]$Command) {
    $logPath = Join-Path $resultsDirectory $LogName
    Write-Host "`n===== $Name =====" -ForegroundColor Cyan
    try {
        # Tee-Object conserva la evidencia y ForEach-Object la fuerza a la terminal.
        # Write-Host evita que la asignacion de Invoke-Stage capture la salida.
        & $Command 2>&1 | Tee-Object -FilePath $logPath | ForEach-Object { Write-Host $_ }
        $code = $LASTEXITCODE
        if ($null -eq $code) { $code = 0 }
        $passed = $code -eq 0
    } catch {
        $_ | Out-File -FilePath $logPath -Append
        $passed = $false
    }
    $results.Add([pscustomobject]@{ Name = $Name; Status = if ($passed) { "PASS" } else { "FAIL" }; Evidence = $logPath })
    if (-not $passed) { Write-Host "$Name FAILED" -ForegroundColor Red }
    return $passed
}

$allPassed = $true
$allPassed = (Invoke-Stage "Unit tests" "unit-tests.txt" { dotnet test (Join-Path $taskRoot "tests\SistemaTareas.UnitTests") --no-restore --logger "console;verbosity=normal" }) -and $allPassed
$allPassed = (Invoke-Stage "Integration tests" "integration-tests.txt" { dotnet test (Join-Path $taskRoot "tests\SistemaTareas.IntegrationTests") --no-restore --logger "console;verbosity=normal" }) -and $allPassed
$allPassed = (Invoke-Stage "Architecture tests" "architecture-tests.txt" { dotnet test (Join-Path $taskRoot "tests\SistemaTareas.ArchitectureTests") --no-restore --logger "console;verbosity=normal" }) -and $allPassed

if (-not $SkipSystem) {
    $allPassed = (Invoke-Stage "System tests" "system-tests.txt" { dotnet test (Join-Path $taskRoot "tests\SistemaTareas.SystemTests") --no-restore --logger "console;verbosity=normal" }) -and $allPassed
} else { $results.Add([pscustomobject]@{ Name = "System tests"; Status = "SKIP"; Evidence = "-" }) }

$allPassed = (Invoke-Stage "Coverage" "coverage.txt" { dotnet test (Join-Path $taskRoot "tests\SistemaTareas.UnitTests") --no-restore --collect:"XPlat Code Coverage" --results-directory $coverageDirectory --logger "console;verbosity=normal" }) -and $allPassed

if (-not $SkipPerformance) {
    $allPassed = (Invoke-Stage "Performance" "performance.txt" { & (Join-Path $PSScriptRoot "run-performance-test.ps1") -DurationSeconds $PerformanceDurationSeconds -ExtraPendingTasks $PerformanceExtraPendingTasks -Port $PerformancePort }) -and $allPassed
} else { $results.Add([pscustomobject]@{ Name = "Performance"; Status = "SKIP"; Evidence = "-" }) }

$summaryPath = Join-Path $resultsDirectory "all-tests-summary.csv"
$results | Export-Csv -NoTypeInformation -Encoding UTF8 -Path $summaryPath
Write-Host "`n===== SUMMARY =====" -ForegroundColor Green
$results | Format-Table -AutoSize
Write-Host "Evidence: $resultsDirectory"
if (-not $allPassed) { Write-Host "Final result: FAILED" -ForegroundColor Red; exit 1 }
Write-Host "Final result: all executed stages passed" -ForegroundColor Green
exit 0
