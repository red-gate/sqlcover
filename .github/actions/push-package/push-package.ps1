$ErrorActionPreference = "Stop"
$filepath=$args[0]
$filename=$args[1]

$package = (Get-ChildItem -Path $filepath -Filter $filename*.nupkg -Recurse).FullName

dotnet nuget add source "$env:NUGET_RG_FEED" `
    --name red-gate-vsts-main-v3 `
    --username "$env:NUGET_USER" `
    --password "$env:NUGET_TOKEN" `
    --store-password-in-clear-text

dotnet nuget push "$package" --api-key "$env:NUGET_TOKEN" --source "$env:NUGET_RG_FEED" --skip-duplicate