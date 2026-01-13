# Build and run sample applications

This documentation depends on

- [Development Environment Setup for GNOME MAUI .NET on Linux](/docs/1-devenv.md)
- [Patch](/docs/2-patch.md)
- [Chicken-egg scenario. Dependency paradox neutralization…](/docs/3-chicken-egg.md)

Open GNOME MAUI ptyxis terminal.

## Build and run MAUI sample app

```bash
cd $GNOMEMAUI/samples/MauiTest1
```

```bash
dotnet build -f net10.0-gnome -v:diag
```

```bash
GSK_RENDERER=opengl dotnet ./bin/Debug/net10.0-gnome/MauiTest1.dll
```

## Build and run MAUI Blazor sample app

```bash
cd $GNOMEMAUI/samples/MauiBlazorApp1
```

```bash
dotnet build -f net10.0-gnome -v:diag
```

```bash
dotnet ./bin/Debug/net10.0-gnome/MauiBlazorApp1.dll
```
