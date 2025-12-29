# GNOME MAUI .NET

![Logo](/assets/nuget/GnomeMaui.svg)

## Overview

GNOME MAUI .NET is a [GirCore](https://gircore.github.io/)-based, modern GTK4/Adwaita .NET MAUI backend for Linux. Its goal is to bring first-class, native GNOME integration to .NET MAUI while preserving the familiar MAUI development model and tooling.

The project provides a dedicated `maui-gnome` workload that introduces the `net10.0-gnome` TFM, allowing MAUI applications to be built and run on Linux using the single-project model, without splitting the app into platform-specific projects.

User interfaces are described using the familiar MAUI XAML dialect, which is mapped to native GNOME widgets and Skia-based rendering primitives. The system is optimized for NativeAOT, delivers ultra-fast startup, and supports Skia CPU and GPU rendering for modern, high-performance graphics on Linux.

GNOME MAUI .NET is a Linux-first MAUI backend that follows GNOME design and technology guidelines while remaining a fully standards-compliant .NET and MAUI stack.

> [!WARNING]
> This project is in an early development stage. Many MAUI features are not yet implemented, but the goal is full MAUI compatibility, even where adaptations are required.

## Samples

Below are two short animated demos showcasing the current state of the project:

- **[MAUI sample app](/samples/MauiTest1)** (Shell navigation, Skia CPU & GPU pages)

https://github.com/user-attachments/assets/ce239643-d9cb-4fcd-8832-1e64a05ef2db

- **[MAUI Blazor sample app](/samples/MauiBlazorApp1)** (XAML host with Blazor integration)

https://github.com/user-attachments/assets/5790be5e-f9af-4da7-bc6d-2de14340341b

## What's ready

| Area | Status |
| - | - |
| `net10.0-gnome` TFM | ✅ Ready |
| `maui-gnome` workload | ✅ Ready |
| Single-project MAUI model | ✅ Ready |
| XAML dialect support | ✅ Ready |
| Blazor integration | ✅ Ready |
| Skia CPU rendering | ✅ Ready |
| Skia GPU rendering | ✅ Ready |

And minimal support for the following MAUI features:

| Area | Status |
| - | - |
| Alert manager | ✅ Ready |
| Shell navigation | 🚧 Basic |
| Label | 🚧 Basic |
| Button | 🚧 Basic |
| Image | 🚧 Basic |

## Skia-based integrations

*Write once, run it on the GPU.*

One of the strongest features of GNOME MAUI .NET is its Skia-based rendering ecosystem, which performs exceptionally well on Linux. This is not traditional widget composition, but a consciously designed, GPU-friendly rendering pipeline.

| Area | Status |
| - | - |
| DrawnUi | 🧩 Almost Ready ~97% |

Sample available at: [samples/MauiDrawnUi1](/samples/MauiDrawnUi1)

**DrawnUi** is a Skia-based, fully drawn UI approach with its own layout and animation pipeline. Instead of managing thousands of native controls, everything is rendered onto a unified surface, resulting in precise control, smooth animations, and high performance. On Linux, this model truly shines: write the code once, and execute it directly on the GPU.

The same rendering philosophy underpins the following integrations, which are actively planned and under development (coming soon):

- **DrawnUi** – fully drawn UI, animations, gestures, effects
- **Mapsui** – maps, layers, large object counts, continuous redraw
- **LiveCharts2** – charts, animations, real-time updates

Together, these components define a graphics stack where MAUI on Linux is not a compromise, but a platform that is powerful and shamelessly fast.

## Developer environment

Detailed instructions for setting up a GNOME MAUI .NET development environment on Linux: [gnomemaui-linux-dev-setup.md](docs/gnomemaui-linux-dev-setup.md)

## License

[![MIT](/assets/shields.io/MIT.svg)](/LICENSE)
