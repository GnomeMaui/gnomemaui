# Chicken-egg scenario. Dependency paradox neutralization…

This documentation depends on

- [Development Environment Setup for GNOME MAUI .NET on Linux](/docs/1-devenv.md)
- [Patch](/docs/2-patch.md)

## Start GNOME Terminal

Open **GNOME Terminal** and select the **GNOME MAUI** profile.

If the GNOME Terminal **GNOME MAUI** profile is open, press `h` and `enter` to return to the devenv home.

## Start GNOME MAUI ptyxis terminal

If the GNOME MAUI ptyxis terminal is open, press `h` and `enter` to return to the devenv home.

You can start the ptyxis terminal in two ways:

**Option 1 - From the desktop launcher**:
The setup script installs a launcher named **GNOME MAUI** with its own icon (`gnomemaui.desktop`). You can start it directly from your GNOME Activities overview.

**Option 2 - From Ptyxis**:
Open **Ptyxis** and select the **GNOME MAUI** profile.
This profile is also created automatically by the setup scripts.

## Installing .NET workloads

```bash
dotnet workload install maui-android wasm-tools --temp-dir ~/.cache

# verify
dotnet workload list
```

### Add local NuGet package source

```bash
dotnet nuget add source "$DOTNET_ROOT/library-packs/" --name "Apps"

# verify
dotnet nuget list source
```

## Build and install GNOME MAUI SDK

### Remove existing SDK

This does not need to be run the first time.

```bash
gnomemaui-remove-sdk.sh
```

### Remove existing NuGet packages

This does not need to be run the first time.

```bash
gnomemaui-nuget-remove.sh
```

### Build chicken-egg SDK for the first time

```bash
gnomemaui-chicken-egg.sh
```

### Install chicken-egg SDK

```bash
gnomemaui-install.sh
```

### Build full version SDK

```bash
gnomemaui-build.sh
```

### Remove chicken-egg SDK

```bash
gnomemaui-remove-sdk.sh
```

### Install full version SDK

```bash
gnomemaui-install.sh
```

From this point on, the TFM for GNOME MAUI .NET exists:

![GNOME MAUI TFM](/assets/TFM.png)

## Next step

To run the sample applications, continue with the [samples](/docs/4-samples.md) documentation.
