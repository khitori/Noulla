namespace Noulla

open Avalonia
open Avalonia.Controls
open Avalonia.Markup.Xaml
open System.Collections.Generic
open Avalonia.Media.Imaging



type MainWindow() as this =
    inherit Window ()

    let _backgrounds = Dictionary<string, Bitmap>()
    let _characters = Dictionary<string, Bitmap>()
    
    let directories =
        dict [
            "backgrounds", "Assets/Backgrounds"
            "characters", "Assets/Characters"
        ]
    
    
    
    do
        this.InitializeComponent()
        this.StretchWindowResolution()

    
    
    // Stretch & center Window to FullScreen
    member private this.StretchWindowResolution() =
        let width, height =
            match this.Screens.Primary with
            | null -> 0.0, 0.0
            | b -> b.Bounds.Width, b.Bounds.Height
        this.Width <- width
        this.Height <- height
        this.WindowStartupLocation <- WindowStartupLocation.CenterScreen
        this.WindowState <- WindowState.FullScreen
        this.SystemDecorations <- SystemDecorations.None
    
    // Stretch & center Background to FullScreen
    member private this.StretchBackgroundImage() =
        let width, height =
            match this.Screens.Primary with
            | null -> 0.0, 0.0
            | b -> b.Bounds.Width, b.Bounds.Height
        this.FindControl<Image>("Background").Width <- width
        this.FindControl<Image>("Background").Height <- height
    
    member private this.GetBackground (dict: Dictionary<string, Bitmap>) (basePath: string) (name: string) =
        
    
    
    
    member private this.InitializeComponent() =
        #if DEBUG
        this.AttachDevTools()
        #endif
        AvaloniaXamlLoader.Load(this)