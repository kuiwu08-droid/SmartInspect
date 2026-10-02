$ErrorActionPreference = 'Stop'
$solutionRoot = Split-Path $PSScriptRoot -Parent
$testArtifacts = Join-Path $env:TEMP ('SmartInspect-verification-' + [guid]::NewGuid().ToString('N'))
& dotnet build (Join-Path $PSScriptRoot 'SmartInspect.Verification.csproj') --artifacts-path $testArtifacts
if ($LASTEXITCODE -ne 0) { throw 'Verification build failed' }
& dotnet (Join-Path $testArtifacts 'bin\SmartInspect.Verification\debug\SmartInspect.Verification.dll') $solutionRoot
if ($LASTEXITCODE -ne 0) { throw "Verification failed; evidence: $testArtifacts" }
Write-Output "Verification passed; evidence: $testArtifacts"
