param(
    [ValidateSet('Debug','Release')]
    [string]$Configuration='Release',
    [switch]$SkipVerification
)

$ErrorActionPreference='Stop'
$solution=Join-Path $PSScriptRoot 'CrescentHawksTools.sln'

dotnet restore $solution
if($LASTEXITCODE -ne 0){throw 'Restore failed.'}

dotnet build $solution --configuration $Configuration --no-restore
if($LASTEXITCODE -ne 0){throw 'Build failed.'}

if(-not $SkipVerification)
{
    dotnet run --project (Join-Path $PSScriptRoot 'tests\InceptionTools.Verification') --configuration $Configuration --no-build
    if($LASTEXITCODE -ne 0){throw 'InceptionTools verification failed.'}

    dotnet run --project (Join-Path $PSScriptRoot 'tests\RevengeTools.Verification') --configuration $Configuration --no-build
    if($LASTEXITCODE -ne 0){throw 'RevengeTools verification failed.'}
}
