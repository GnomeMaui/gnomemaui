# Development Environment Setup for GNOME MAUI .NET on Linux

The development environment currently does not support Flatpak, Snap, and similar packages. These are isolated environments. Use native Linux installation. You will find instructions below.

The development environment requires a minimum of GNOME 48.

## Environment Variables

Two mandatory environment variables must be set. The documentation and the project use these variables to determine the location of the development environment:

- GNOMEMAUIROOT - the root directory of GNOME MAUI .NET. This is where the linked projects and source code will be placed. This can be freely changed.
- GNOMEMAUI - the development environment of GNOME MAUI .NET. This is where we clone the development environment.

Add the following lines to the end of your `.bashrc`:

```bash
# Freely changeable location
export GNOMEMAUIROOT=$HOME/gnomemaui

# Mandatory variable, cannot be changed
export GNOMEMAUI=$GNOMEMAUIROOT/gnomemaui
```

> [!CAUTION]
> You need to log out and back in for the environment variables to take effect!

## Install the required dependencies

### Supported Operating Systems

- Arch Linux
- Debian 13
- Fedora 43
- OpenSUSE Tumbleweed
- Ubuntu 25.10

Follow the instructions found here:

[https://github.com/czirok/devenv?tab=readme-ov-file#dependencies](https://github.com/czirok/devenv?tab=readme-ov-file#dependencies)

## Setting up the development environment

Open Ptyxis terminal.

Create the GNOMEMAUIROOT directory:

```bash
mkdir $GNOMEMAUIROOT
```

```bash
cd $GNOMEMAUIROOT
```

Clone the GNOME MAUI repository:

```bash
git clone https://github.com/gnomemaui/gnomemaui.git
```

```bash
cd $GNOMEMAUI
```

Download devenv

```bash
wget https://github.com/czirok/devenv/releases/download/v2026.01.13/devenv.tar.bz2
```

Safe extract the archive:

```bash
tar xjfv devenv.tar.bz2 --skip-old-files
```

Check dependencies:

```bash
.vscode/.linux/install.sh
```

If you see that everything is checked, then everything is fine. (This example was run on Arch Linux):

```bash
[✓] Trash CLI found (version: 0.24.5.26)
[✓] BC found (version: bc 1.08.2)
[✓] Curl found (version: curl 8.17.0 (x86_64-pc-linux-gnu) libcurl/8.17.0 OpenSSL/3.6.0 zlib/1.3.1 brotli/1.1.0 zstd/1.5.7 libidn2/2.3.8 libpsl/0.21.5 libssh2/1.11.1 nghttp2/1.68.0 nghttp3/1.13.1 mit-krb5/1.21.3)
[✓] Bash found (version: GNU bash, 5.3.9(1)-release (x86_64-pc-linux-gnu) version)
[✓] Visual Studio Code found (version: 1.107.1)
[✓] Ptyxis found (version: Ptyxis 49.2)
[✓] All dependencies are installed!
```

If something is not installed, you can find the documentation required for installation here: [devenv](https://github.com/czirok/devenv?tab=readme-ov-file#dependencies)

When everything is in order, the next step is to install the development environment:

```bash
.vscode/.linux/install.sh --all
```

You should see this or something similar:

```bash
[✓] GNOMEMAUI="/home/user/gnomemaui/gnomemaui" variable added to /home/user/.bash_profile successfully
[✓] .bashrc created and updated with GNOMEMAUI
[✓] Generated 21 theme palettes in /home/user/.local/share/org.gnome.Ptyxis/palettes
[✓] Ptyxis profile configuration created
[✓] Ptyxis profile configured
[✓] Icon installed as gnomemaui.svg
[✓] Desktop file installed as gnomemaui.desktop
[!] 3 fonts were already installed
[✓] Icon cache updated
[✓] Desktop database updated
[✓] Font cache updated
[✓] Configuration created and updated with GNOMEMAUI
[✓] Oh My Posh installed to /home/user/gnomemaui/gnomemaui/.vscode/.linux
[✓] VSCode settings updated with new colors
[✓] .NET 10 SDK installed
[✓] .NET 9 Runtime installed
[✓] .NET 9 ASP.NET Core Runtime installed
[✓] NuGet source configured
[✓] .NET tools installed
[✓] Installation completed successfully!
[!] You may need to log out and back in for the GNOMEMAUI environment variable to take effect, if you changed the git root directory.
```

> [!CAUTION]
> You need to log out again and back in for the environment variables to take effect!

## Next step

You can now continue with the [patch](/docs/2-patch.md) documentation.
