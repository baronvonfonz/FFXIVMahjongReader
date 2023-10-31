For my sake...
# Links

[Packager](https://github.com/goatcorp/DalamudPackager)

## Steps

In `src/MahjongReader`
```
dotnet build --configuration Release
```

Then use the GitHub command line tool:

```
gh release create v.1.1.0 bin\Release\MahjongReader\latest.zip --title "<title>" --notes "<release_notes>"
```