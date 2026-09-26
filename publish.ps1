param(
    [Parameter(Mandatory=$true)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$pwaProject = Join-Path $root 'CloudMartAutoShop.Pwa\CloudMartAutoShop.Pwa.csproj'
$versionJson = Join-Path $root 'CloudMartAutoShop.Pwa\wwwroot\version.json'
$mainLayout = Join-Path $root 'CloudMartAutoShop.Pwa\Layout\MainLayout.razor'
$serviceWorker = Join-Path $root 'CloudMartAutoShop.Pwa\wwwroot\service-worker.published.js'         
$publishDir = 'C:\Deploy\CloudMartAutoShop\Pwa'

Write-Host "Preparing CloudMart Auto Shop v$Version..."

[xml]$project = Get-Content $pwaProject
$group = $project.Project.PropertyGroup |
    Where-Object { $_.Version } |
    Select-Object -First 1

$group.Version = $Version
$group.AssemblyVersion = "$Version.0"
$group.FileVersion = "$Version.0"
$project.Save($pwaProject)
$layoutContent = Get-Content $mainLayout -Raw
$layoutContent = $layoutContent -replace `
    '<small>v\d+\.\d+\.\d+</small>', `
    "<small>v$Version</small>"

$layoutContent = $layoutContent -replace `
    'Auto Shop &bull; v\d+\.\d+\.\d+', `
    "Auto Shop &bull; v$Version"

$layoutContent = $layoutContent -replace `
    'private const string CurrentVersion = "\d+\.\d+\.\d+";', `
    "private const string CurrentVersion = `"$Version`";"

Set-Content $mainLayout $layoutContent -Encoding UTF8
$serviceWorkerContent = Get-Content $serviceWorker -Raw

$serviceWorkerContent = $serviceWorkerContent -replace `
    '(?m)^// CloudMart Auto Shop release \d+\.\d+\.\d+\r?\n', `
    ''

$serviceWorkerContent = "// CloudMart Auto Shop release $Version`r`n" +
    $serviceWorkerContent

Set-Content $serviceWorker $serviceWorkerContent -Encoding UTF8

@{ version = $Version } |
    ConvertTo-Json |
    Set-Content $versionJson -Encoding UTF8

if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}

dotnet publish $pwaProject -c Release -o $publishDir

if ($LASTEXITCODE -ne 0) {
    throw 'dotnet publish failed.'
}

$publishedVersion =
    Get-Content (Join-Path $publishDir 'wwwroot\version.json') |
    ConvertFrom-Json

if ($publishedVersion.version -ne $Version) {
    throw 'Published version.json does not match requested version.'
}

aws s3 sync `
    (Join-Path $publishDir 'wwwroot') `
    's3://cloudmartdata-pwa/autoshop/' `
    --delete

if ($LASTEXITCODE -ne 0) {
    throw 'S3 sync failed.'
}

aws cloudfront create-invalidation `
    --distribution-id E20FZCD865P0IN `
    --paths '/autoshop/*'

if ($LASTEXITCODE -ne 0) {
    throw 'CloudFront invalidation failed.'
}

Write-Host "Auto Shop v$Version PWA deployed successfully."
Write-Host 'Note: API/database deployment is intentionally separate.'