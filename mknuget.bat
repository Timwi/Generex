dotnet pack Src/Generex.csproj --configuration Release -o Publish
dotnet nuget push Publish\*.nupkg -s https://api.nuget.org/v3/index.json
del Publish\**
