param(
)

[string] $RepoRoot = Resolve-Path "$PSScriptRoot\.."

$StartingLocation = Get-Location
Set-Location -Path $RepoRoot

Write-Host "Test release..."
try
{
    dotnet test "$RepoRoot\src\FrameworkDetector.sln"
    if (!$?) {
    	throw 'Tests failed!'
    }
}
finally
{
    Set-Location -Path "$StartingLocation"
}
