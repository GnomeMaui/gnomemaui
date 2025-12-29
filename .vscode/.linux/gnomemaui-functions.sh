
source "$GNOMEMAUI/.vscode/.linux/scripts/colors.sh"

detect_gnomemaui() {
    if [ -z "$GNOMEMAUI" ]; then
        print_error "GNOMEMAUI environment variable is not set."
        return 1
    fi

    return 0
}

stop_dotnet_processes() {
    print_step "Stopping any running dotnet processes"
    killall dotnet >/dev/null 2>&1 || true 
}

delete_nuget_packages() {
    print_step "Delete GNOME MAUI nuget packages"

    rm -f $DOTNET_ROOT/library-packs/GnomeMaui.* >/dev/null 2>&1 || true 
    rm -f $DOTNET_ROOT/library-packs/gnomemaui.* >/dev/null 2>&1 || true 
    if [ -d "$HOME/.nuget/packages/gnomemaui.controls" ]; then
        rm -rf "$HOME/.nuget/packages/gnomemaui.controls"
    fi
    if [ -d "$HOME/.nuget/packages/gnomemaui.controls.core" ]; then
        rm -rf "$HOME/.nuget/packages/gnomemaui.controls.core"
    fi
    if [ -d "$HOME/.nuget/packages/gnomemaui.core" ]; then
        rm -rf "$HOME/.nuget/packages/gnomemaui.core"
    fi
    if [ -d "$HOME/.nuget/packages/gnomemaui.essentials" ]; then
        rm -rf "$HOME/.nuget/packages/gnomemaui.essentials"
    fi
    if [ -d "$HOME/.nuget/packages/gnomemaui.graphics" ]; then
        rm -rf "$HOME/.nuget/packages/gnomemaui.graphics"
    fi
    if [ -d "$HOME/.nuget/packages/gnomemaui.skia" ]; then
        rm -rf "$HOME/.nuget/packages/gnomemaui.skia"
    fi
    if [ -d "$HOME/.nuget/packages/gnomemaui.tools" ]; then
        rm -rf "$HOME/.nuget/packages/gnomemaui.tools"
    fi
    if [ -d "$HOME/.nuget/packages/gnomemaui.sdk" ]; then
        rm -rf "$HOME/.nuget/packages/gnomemaui.sdk"
    fi
    if [ -d "$HOME/.nuget/packages/gnomemaui.sdk.manifest" ]; then
        rm -rf "$HOME/.nuget/packages/gnomemaui.sdk.manifest"
    fi
    if [ -d "$HOME/.nuget/packages/gnomemaui.controls.build.tasks" ]; then
        rm -rf "$HOME/.nuget/packages/gnomemaui.controls.build.tasks"
    fi
    if [ -d "$HOME/.nuget/packages/gnomemaui.controls.xaml" ]; then
        rm -rf "$HOME/.nuget/packages/gnomemaui.controls.xaml"
    fi
    if [ -d "$HOME/.nuget/packages/gnomemaui.skiasharp.core" ]; then
        rm -rf "$HOME/.nuget/packages/gnomemaui.skiasharp.core"
    fi
    if [ -d "$HOME/.nuget/packages/gnomemaui.skiasharp.controls" ]; then
        rm -rf "$HOME/.nuget/packages/gnomemaui.skiasharp.controls"
    fi
    if [ -d "$HOME/.nuget/packages/gnomemaui.blazor" ]; then
        rm -rf "$HOME/.nuget/packages/gnomemaui.blazor"
    fi
        if [ -d "$HOME/.nuget/packages/gnomemaui.drawnui" ]; then
        rm -rf "$HOME/.nuget/packages/gnomemaui.drawnui"
    fi
}

delete_objbin_folders() {
    print_step "Delete obj and bin folders from GNOME MAUI source"

    if [ -d "$GNOMEMAUI/src/GnomeMaui.Sdk.Manifest/obj" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Sdk.Manifest/obj"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Core/obj" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Core/obj"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Controls/obj" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Controls/obj"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Controls.Core/obj" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Controls.Core/obj"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Controls.Xaml/obj" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Controls.Xaml/obj"
    fi    
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Essentials/obj" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Essentials/obj"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Graphics/obj" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Graphics/obj"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.SkiaSharp.Core/obj" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.SkiaSharp.Core/obj"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.SkiaSharp.Controls/obj" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.SkiaSharp.Controls/obj"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Tools/obj" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Tools/obj"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Sdk/obj" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Sdk/obj"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Controls.Build.Tasks/obj" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Controls.Build.Tasks/obj"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Controls.Xaml/obj" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Controls.Xaml/obj"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Blazor/obj" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Blazor/obj"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.DrawnUi/obj" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.DrawnUi/obj"
    fi

    if [ -d "$GNOMEMAUI/src/GnomeMaui.Sdk.Manifest/bin" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Sdk.Manifest/bin"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Core/bin" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Core/bin"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Controls/bin" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Controls/bin"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Controls.Core/bin" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Controls.Core/bin"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Controls.Xaml/bin" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Controls.Xaml/bin"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Essentials/bin" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Essentials/bin"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Graphics/bin" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Graphics/bin"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.SkiaSharp.Core/bin" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.SkiaSharp.Core/bin"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.SkiaSharp.Controls/bin" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.SkiaSharp.Controls/bin"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Tools/bin" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Tools/bin"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Sdk/bin" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Sdk/bin"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Controls.Build.Tasks/bin" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Controls.Build.Tasks/bin"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Controls.Xaml/bin" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Controls.Xaml/bin"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.Blazor/bin" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.Blazor/bin"
    fi
    if [ -d "$GNOMEMAUI/src/GnomeMaui.DrawnUi/bin" ]; then
        rm -rf "$GNOMEMAUI/src/GnomeMaui.DrawnUi/bin"
    fi
}
