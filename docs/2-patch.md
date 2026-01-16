# Patch

> [!NOTE]
> This documentation depends on the [# Development Environment Setup for GNOME MAUI .NET on Linux](/docs/1-devenv.md) documentation.

## Start preferred Terminal

Depending on the terminal you have setu up, choose one of the two options below:

### Start GNOME Terminal

Open **GNOME Terminal** and select the **GNOME MAUI** profile.

### Start GNOME MAUI ptyxis terminal

You can start the ptyxis terminal in two ways:

**Option 1 - From the desktop launcher**:
The setup script installs a launcher named **GNOME MAUI** with its own icon (`gnomemaui.desktop`). You can start it directly from your GNOME Activities overview.

**Option 2 - From Ptyxis**:
Open **Ptyxis** and select the **GNOME MAUI** profile.
This profile is also created automatically by the setup scripts.

## NodeJS installation

If asked, say yes to install the required NodeJS version:

```bash
Can't find an installed Node version matching v24.12.0.
Do you want to install it? answer [y/N]: y
```

Install the packages:

```bash
pnpm install
```

Navigate to the GNOME MAUI root directory:

```bash
cd $GNOMEMAUI/..
```

## MAUI

The current patch requires this exact version, do not change it:

```bash
git init maui-10.0.20.sr2
cd maui-10.0.20.sr2
git remote add origin https://github.com/dotnet/maui.git
git fetch --depth=1 origin 0d1705adc4a6b4ec531e316ec956755abbe059c5
git checkout FETCH_HEAD
# verify
git rev-parse HEAD
# 0d1705adc4a6b4ec531e316ec956755abbe059c5
```

```bash
cd $GNOMEMAUI/..
```

```bash
git clone --branch release/3.119.1 --single-branch --depth 1 https://github.com/mono/SkiaSharp.git SkiaSharp-3.119.1

git clone --depth 1 https://github.com/taublast/DrawnUi.git

git clone --depth 1 https://github.com/taublast/AppoMobi.Maui.Gestures.git

git clone --depth 1 https://github.com/taublast/AppoMobi.Specials.git
```

### Symlinks

```bash
cd $GNOMEMAUI/ext

ln -s $GNOMEMAUI/../maui-10.0.20.sr2 maui

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

# git apply --check $GNOMEMAUI/ext/maui-10.0.20.sr2.patch

git apply $GNOMEMAUI/ext/maui-10.0.20.sr2.patch

cd $GNOMEMAUI/ext/drawnui/drawnui

git apply $GNOMEMAUI/ext/drawnui.patch

cd $GNOMEMAUI/ext/drawnui/drawnui-gestures

git apply $GNOMEMAUI/ext/drawnui-gestures.patch

```

## Next step

You can continue with the GNOME MAUI .NET [Chicken-and-egg SDK](/docs/3-chicken-egg.md) documentation.
