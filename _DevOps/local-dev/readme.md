# Commands

``` bash
dotnet pack

dotnet pack --output ./nupkgs --configuration Release
```

``` bash
dotnet nuget push "./nupkgs/*.nupkg" --api-key <> --source https://api.nuget.org/v3/index.json
```
