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
    dotnet nuget push -k $Env:NUGET_API_KEY -s https://api.nuget.org/v3/index.json ./Envisia.React.Extensions/bin/Release/Envisia.React.Extensions.$Version.nupkg
    
    dotnet nuget push -k $Env:NEXUS_NUGET_KEY -s https://nexus.envisia.io/repository/envisia-nuget/api/v2/package/ ./Envisia.Webpack.Extensions/bin/Release/Envisia.Webpack.Extensions.$Version.nupkg
    dotnet nuget push -k $Env:NUGET_API_KEY -s https://api.nuget.org/v3/index.json ./Envisia.Webpack.Extensions/bin/Release/Envisia.Webpack.Extensions.$Version.nupkg
        
    dotnet nuget push -k $Env:NEXUS_NUGET_KEY -s https://nexus.envisia.io/repository/envisia-nuget/api/v2/package/ ./Envisia.React.Engine.V8/bin/Release/Envisia.React.Engine.V8.$Version.nupkg
    dotnet nuget push -k $Env:NUGET_API_KEY -s https://api.nuget.org/v3/index.json ./Envisia.React.Engine.V8/bin/Release/Envisia.React.Engine.V8.$Version.nupkg

    git tag v$Version
    git push --tags
}
