param(
    [string]$ProjectPath = ".\\GameLibrary.Api.csproj",
    [string]$Configuration = "Release",
    [string]$OutputRoot = ".\\publish",
    [string[]]$Runtimes = @("linux-x64", "linux-arm64", "win-x64"),
    [bool]$SelfContained = $true
)

$ErrorActionPreference = "Stop"

Write-Host "Project: $ProjectPath"
Write-Host "Configuration: $Configuration"
Write-Host "Output root: $OutputRoot"
Write-Host "Runtimes: $($Runtimes -join ', ')"
Write-Host "SelfContained: $SelfContained"

foreach ($rid in $Runtimes) {
    $outDir = Join-Path $OutputRoot $rid
    Write-Host ""
    Write-Host "Publishing runtime $rid -> $outDir"

    dotnet publish $ProjectPath `
        -c $Configuration `
        -r $rid `
        --self-contained $SelfContained `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:PublishTrimmed=false `
        -p:InvariantGlobalization=true `
        -o $outDir
}

Write-Host ""
Write-Host "Publish completed."
