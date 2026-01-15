# Development Environment Setup for GNOME MAUI .NET on Linux

The development environment currently does not support Flatpak, Snap, and similar packages. These are isolated environments. Use native Linux installation. You will find instructions below.

The development environment requires a minimum of GNOME 48. The current GNOME MAUI .NET development environment still works with GNOME 46, but GNOME 48 or newer is required for future development.

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

Open Ptyxis terminal or GNOME Terminal.

Create the GNOMEMAUIROOT directory:

```bash
mkdir $GNOMEMAUIROOT
```

```bash
cd $GNOMEMAUIROOT
```

Clone the GNOME MAUI repository:

```bash
git clone --depth 1 https://github.com/gnomemaui/gnomemaui.git
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

On Ubuntu 24.04 LTS, Ptyxis is not available, so this output will appear instead. A GNOME Terminal can be used instead of Ptyxis.

```bash
[✓] Trash CLI found (version: 0.23.11.10)
[✓] BC found (version: bc 1.07.1)
[✓] Curl found (version: curl 8.5.0 (x86_64-pc-linux-gnu) libcurl/8.5.0 OpenSSL/3.0.13 zlib/1.3 brotli/1.1.0 zstd/1.5.5 libidn2/2.3.7 libpsl/0.21.2 (+libidn2/2.3.7) libssh/0.10.6/openssl/zlib nghttp2/1.59.0 librtmp/2.3 OpenLDAP/2.6.7)
[✓] Bash found (version: GNU bash, version 5.2.21(1)-release (x86_64-pc-linux-gnu))
[✓] Visual Studio Code found (version: 1.108.0)
[✗] Ptyxis not found - install Ptyxis terminal
[✗] Some dependencies are missing. Please install them before running the installer.
```

> [!CAUTION]
> Flatpak Visual Studio Code is not supported. Please install the native version: [Ubuntu VS Code](https://github.com/czirok/devenv?tab=readme-ov-file#ubuntu-plucky-puffin-2510-and-debian-trixie-13)

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

If you are using GNOME Terminal, follow the instructions found here: [GNOME Terminal manual setup](/docs/1.1-gnome-terminal-setup.md)

You can now continue with the [patch](/docs/2-patch.md) documentation.
