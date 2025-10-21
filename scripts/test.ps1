param(
    [string]$Configuration = "Debug"
)

[string] $RepoRoot = Resolve-Path "$PSScriptRoot\.."

$StartingLocation = Get-Location
Set-Location -Path $RepoRoot

Write-Host "Test FrameworkDetector..."
try {
    dotnet test --configuration $Configuration "$RepoRoot\src\FrameworkDetector.sln"
    if (!$?) {
    	throw 'Tests failed!'
    }
} finally {
    Set-Location -Path "$StartingLocation"
}
