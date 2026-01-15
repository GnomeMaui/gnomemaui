# GNOME MAUI .NET

![Logo](/assets/nuget/GnomeMaui.svg)

## Overview

GNOME MAUI .NET is a modern, legacy-free .NET MAUI backend for Linux, built on [GirCore](https://gircore.github.io/) with native GTK4 and Adwaita integration.

> [!NOTE]
> The project is not intended to carry forward technologies from the past, therefore it **does not include**:
>
> * a Xamarin.Forms compatibility layer
> * obsolete APIs or features
> * TwoPaneView (will be implemented when the first foldable GNOME phone ships 😉)

Its goal is to deliver first-class, native GNOME integration for .NET MAUI while preserving the familiar MAUI development model and tooling.

https://github.com/user-attachments/assets/c5441240-55d0-4127-9467-c097ce6bb635

The project provides a dedicated `maui-gnome` workload that introduces the `net10.0-gnome` TFM, allowing MAUI applications to be built and run on Linux using the single-project model, without splitting the app into platform-specific projects.

User interfaces are described using the familiar MAUI XAML dialect, which is mapped to native GNOME widgets and SkiaSharp-based rendering primitives. The system is optimized for NativeAOT, delivers ultra-fast startup, and supports SkiaSharp CPU and GPU rendering for modern, high-performance graphics on Linux.

GNOME MAUI .NET is a Linux-first MAUI backend that follows GNOME design and technology guidelines while maintaining full compliance with .NET and MAUI standards.

> [!IMPORTANT]
> The project is currently in an active validation phase.
>
> Comprehensive testing of the MAUI feature matrix is in progress, along with parallel execution and verification of **134 official MAUI sample projects**.
>
> In parallel, the [GnomeMaui Samples](https://github.com/GnomeMaui/gnomemaui-samples) repository is being continuously updated with successfully validated examples, while ongoing minor fixes, fine-tuning, and stabilization work take place.

## Getting started

TODO: Create documentation for end users to install GNOME MAUI .NET SDK and workload.

## Developer environment setup

The development environment currently does not support Flatpak, Snap, and similar packages. These are isolated environments. Use native Linux installation. You will find instructions below.

Follow the [Development Environment Setup for GNOME MAUI .NET on Linux](/docs/1-devenv.md) documentation to set up your development environment.

## Changelog

See the [.github/releases](.github/releases) folder for the changelog.

## License

[![MIT](/assets/shields.io/MIT.svg)](/LICENSE)
