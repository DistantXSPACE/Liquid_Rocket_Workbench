[CmdletBinding()]
param(
    [ValidateNotNullOrEmpty()]
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repositoryRoot `
    "src\LiquidRocketWorkbench.App\LiquidRocketWorkbench.App.csproj"
$publishRoot = Join-Path $repositoryRoot "publish"
$artifactName = "LiquidRocketWorkbench-$Version-win-x64"
$outputDirectory = Join-Path $publishRoot $artifactName
$archivePath = Join-Path $publishRoot "$artifactName.zip"
$checksumPath = "$archivePath.sha256"
$expectedExecutable = Join-Path $outputDirectory "LiquidRocketWorkbench.exe"

if (Test-Path -LiteralPath $outputDirectory)
{
    Remove-Item -LiteralPath $outputDirectory -Recurse -Force
}

foreach ($path in @($archivePath, $checksumPath))
{
    if (Test-Path -LiteralPath $path)
    {
        Remove-Item -LiteralPath $path -Force
    }
}

New-Item -ItemType Directory -Path $publishRoot -Force | Out-Null

dotnet publish $projectPath `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $outputDirectory `
    -p:Version=$Version `
    -p:DebugSymbols=false `
    -p:DebugType=None

if ($LASTEXITCODE -ne 0)
{
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

if (-not (Test-Path -LiteralPath $expectedExecutable))
{
    throw "The expected executable was not published: $expectedExecutable"
}

Copy-Item -LiteralPath (Join-Path $repositoryRoot "README.md") `
    -Destination $outputDirectory
New-Item -ItemType Directory `
    -Path (Join-Path $outputDirectory "docs") `
    -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $repositoryRoot "docs\release.md") `
    -Destination (Join-Path $outputDirectory "docs")
Copy-Item -LiteralPath (Join-Path $repositoryRoot "docs\references.md") `
    -Destination (Join-Path $outputDirectory "docs")

$screenshotSource = Join-Path $repositoryRoot "docs\screenshots"
if (-not (Test-Path -LiteralPath $screenshotSource -PathType Container))
{
    throw "Release screenshots are missing. Run scripts\Capture-ReleaseScreenshots.ps1 first."
}

Copy-Item -LiteralPath $screenshotSource `
    -Destination (Join-Path $outputDirectory "docs") `
    -Recurse

Compress-Archive -LiteralPath $outputDirectory `
    -DestinationPath $archivePath `
    -CompressionLevel Optimal

$checksum = Get-FileHash -LiteralPath $archivePath -Algorithm SHA256
$checksumLine = "$($checksum.Hash.ToLowerInvariant())  $([IO.Path]::GetFileName($archivePath))"
Set-Content -LiteralPath $checksumPath `
    -Value $checksumLine `
    -Encoding ascii

$publishedFiles = Get-ChildItem -LiteralPath $outputDirectory `
    -Recurse `
    -File
$publishedBytes = (
    $publishedFiles | Measure-Object -Property Length -Sum
).Sum

[pscustomobject]@{
    Version = $Version
    Executable = $expectedExecutable
    Folder = $outputDirectory
    FileCount = $publishedFiles.Count
    FolderBytes = $publishedBytes
    Archive = $archivePath
    ArchiveBytes = (Get-Item -LiteralPath $archivePath).Length
    Sha256 = $checksum.Hash.ToLowerInvariant()
}
