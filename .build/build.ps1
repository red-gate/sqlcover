[CmdletBinding()]
param(
    [string] $Configuration = 'Release',
    [string] $BranchName = 'dev',
    [bool] $IsDefaultBranch = $false,
    [string] $NugetFeedUrl,
    [string] $NugetFeedApiKey,
    [string] $SigningServiceUrl = 'https://signingservice.red-gate.com/Sign',
    [string] $GithubAPIToken
)

$RootDir = "$PsScriptRoot\.." | Resolve-Path
Write-Host "RootDirectory set $RootDir"
$OutputDir = "$RootDir\.output\$Configuration"
$LogsDir = "$OutputDir\logs"
$NugetPackageOutputDir = "$OutputDir\nugetpackages"
$Solution = "$RootDir\src\SQLCover\SQLCover.sln"
$NugetExe = "$PSScriptRoot\packages\Nuget.CommandLine\tools\Nuget.exe" | Resolve-Path
$Repo = "sqlcover"
$BuildRoot = "$PsScriptRoot" | Resolve-Path

# Load task definitions from tasks/
#Get-ChildItem -Path $PsScriptRoot\tasks -Filter *.tasks.ps1 | ForEach-Object {
  #. $_.FullName
#}
task CreateFolders {
    #New-Item $OutputDir -ItemType Directory -Force | Out-Null
    #New-Item $LogsDir -ItemType Directory -Force | Out-Null
    New-Item $NugetPackageOutputDir -ItemType Directory -Force | Out-Null
}


# Synopsis: Restore the nuget packages of the Visual Studio solution
task RestoreNugetPackages {
    exec {
        & $NugetExe restore "$Solution" -Verbosity detailed
    }
}

# Synopsis: A task that makes sure our initialization tasks have been run before we can do anything useful
task Init CreateFolders, RestoreNugetPackages
#, GenerateVersionInformation

# Synopsis: Compile the Visual Studio solution
task Compile Init, {
    Set-Alias msbuild (Resolve-MSBuild -MinimumVersion 15.0)
    try {
        exec {
            msbuild `
                "$Solution" `
                /maxcpucount `
                /nodereuse:false `
                /target:Build `
                /p:Configuration=$Configuration `
        }
    } finally {
        #TeamCity-PublishArtifact "$LogsDir\_msbuild.log.* => logs/msbuild.$Configuration.logs.zip"
        Write-Host "Logs generated after build"
    }
}

# Synopsis: Build the project.
task Build Init, Compile

# Synopsis: By default, Call the 'Build' task
task . Build