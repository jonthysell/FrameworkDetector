param(
    [string]$Configuration = "Debug"
)

[string] $RepoRoot = Resolve-Path "$PSScriptRoot\.."

$StartingLocation = Get-Location
Set-Location -Path $RepoRoot

Write-Host "Build FrameworkDetector..."
try {
    dotnet build --configuration $Configuration "$RepoRoot\src\FrameworkDetector.sln"
    if (!$?) {
    	throw 'Build failed!'
    }
} finally {
    Set-Location -Path "$StartingLocation"
}
