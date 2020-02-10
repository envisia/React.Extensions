param([string] $Version)

if(git status --porcelain |Where {$_ -match '^\?\?'}){
    # untracked files exist
    Write-Output "Found untracked files"
}
elseif(git status --porcelain |Where {$_ -notmatch '^\?\?'}) {
    # uncommitted changes
    Write-Output "Found changed files"
}
else {
    # tree is clean
    dotnet pack -c Release /p:Version=$Version
    
    dotnet nuget push -k $Env:NEXUS_NUGET_KEY -s https://nexus.envisia.io/repository/envisia-nuget/api/v2/package/ ./Envisia.React.Extensions/bin/Release/Envisia.React.Extensions.$Version.nupkg
    
     git tag v$Version
     git push --tags
}