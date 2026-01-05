# GNOME MAUI Linux developer environment setup

## Requirements

![GNOME 48+](/assets/shields.io/GNOME_48plus.svg)

Linux distribution packages for dotnet do not include the MAUI manifest and SDK workloads, so you must use the official Microsoft installer script. If you already have dotnet installed on your machine, remove it first.

Tested on

- Arch Linux
- Debian 13
- Fedora 43
- Ubuntu 25.10
- Raspbian 13

## Installing .NET

```bash
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh

chmod +x ./dotnet-install.sh

# x86 host
./dotnet-install.sh --channel "10.0" --architecture x64 --os linux

# arm64 host
./dotnet-install.sh --channel "10.0" --architecture arm64 --os linux

# workload fallback
# x86 host
./dotnet-install.sh --channel "9.0" --architecture x64 --runtime dotnet --os linux

# arm64 host
./dotnet-install.sh --channel "9.0" --architecture arm64 --runtime dotnet --os linux
```

## Setting environment variables

Add the following lines to the end of your `.bashrc`:

```bash
export GNOMEMAUI=$HOME/gnomemaui/gnomemaui
export GNOMEMAUILINUX=$GNOMEMAUI/.vscode/.linux
export PATH=$PATH:$GNOMEMAUILINUX

export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
```

> [!CAUTION]
> **VERY IMPORTANT!** All the environment variables listed below are required for the GNOME MAUI development environment, not just `GNOMEMAUI`. Log out of GNOME now and log back in so the environment variables (including `GNOMEMAUI`, `GNOMEMAUILINUX`, `DOTNET_ROOT`, and the updated `PATH`) take effect. Then return here.

Then run the following commands to verify the environment variables:

```bash
echo $GNOMEMAUI
/home/user/gnomemaui/gnomemaui

echo $GNOMEMAUILINUX
/home/user/gnomemaui/gnomemaui/.vscode/.linux

echo $DOTNET_ROOT
/home/user/.dotnet
```

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

## Cloning the GNOME MAUI repository

Create the GNOME MAUI directory; it will contain the repository and the MAUI and SkiaSharp sources.

```bash
mkdir $HOME/gnomemaui

cd $HOME/gnomemaui

git clone https://github.com/gnomemaui/gnomemaui.git

git clone --branch release/10.0.1xx-sr1 --single-branch --depth 1 https://github.com/dotnet/maui.git maui-10.0.1xx-sr1

git clone --branch release/3.119.1 --single-branch --depth 1 https://github.com/mono/SkiaSharp.git SkiaSharp-3.119.1

git clone --depth 1 https://github.com/taublast/DrawnUi.git

git clone --depth 1 https://github.com/taublast/AppoMobi.Maui.Gestures.git

git clone --depth 1 https://github.com/taublast/AppoMobi.Specials.git
```

### Symlinks

```bash
cd $GNOMEMAUI/ext

ln -s $GNOMEMAUI/../maui-10.0.1xx-sr1 maui

ln -s $GNOMEMAUI/../SkiaSharp-3.119.1 skiasharp

mkdir $GNOMEMAUI/ext/drawnui

cd $GNOMEMAUI/ext/drawnui

ln -s $GNOMEMAUI/../DrawnUi drawnui

ln -s $GNOMEMAUI/../AppoMobi.Maui.Gestures drawnui-gestures

ln -s $GNOMEMAUI/../AppoMobi.Specials drawnui-specials
```

### Patch MAUI

```bash
cd $GNOMEMAUI/ext/maui

git apply $GNOMEMAUI/ext/maui-10.0.1xx-sr1.1.patch

cd $GNOMEMAUI/ext/drawnui/drawnui

git apply $GNOMEMAUI/ext/drawnui.patch

cd $GNOMEMAUI/ext/drawnui/drawnui-gestures

git apply $GNOMEMAUI/ext/drawnui-gestures.patch

```

## Download necessary scripts

```bash
mkdir $GNOMEMAUILINUX/scripts

wget "https://raw.githubusercontent.com/czirok/devenv/refs/heads/main/.vscode/.linux/scripts/colors.sh" -O $GNOMEMAUILINUX/scripts/colors.sh
```

## Build and install the GNOME MAUI SDK

```bash
cd $GNOMEMAUI

gnomemaui-remove-sdk.sh
gnomemaui-nuget-remove.sh
gnomemaui-chicken-egg.sh
gnomemaui-install.sh
gnomemaui-build.sh
gnomemaui-remove-sdk.sh
gnomemaui-install.sh
```

```bash
dotnet workload list | grep maui
maui-android               10.0.1/10.0.100        SDK 10.0.100       
maui-gnome                 10.0.1/10.0.100        SDK 10.0.100  
```

## Build and run a GNOME MAUI sample app

```bash
cd $GNOMEMAUI/samples/MauiTest1
dotnet build -f net10.0-gnome -v:diag
GSK_RENDERER=opengl dotnet ./bin/Debug/net10.0-gnome/MauiTest1.dll
```

## Build and run a GNOME MAUI Blazor sample app

```bash
cd $GNOMEMAUI/samples/MauiBlazorApp1
dotnet build -f net10.0-gnome -v:diag
dotnet ./bin/Debug/net10.0-gnome/MauiBlazorApp1.dll
```

## Build and run a GNOME MAUI DrawnUi sample app

```bash
cd $GNOMEMAUI/samples/MauiDrawnUi1
dotnet build -f net10.0-gnome -v:diag
GSK_RENDERER=opengl dotnet ./bin/Debug/net10.0-gnome/MauiDrawnUi1.dll
```
