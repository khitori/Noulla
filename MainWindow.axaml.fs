namespace Noulla

open System
open System.IO
open Avalonia
open Avalonia.Controls
open Avalonia.Markup.Xaml
open System.Collections.Generic
open Avalonia.Media.Imaging
open Avalonia.Platform



type MainWindow() as this =
    inherit Window ()

    let _backgrounds = Dictionary<string, Bitmap>()
    let _characters = Dictionary<string, Bitmap>()
    
    let paths =
        Map.ofList [
            "backgrounds", (_backgrounds, "Assets/Backgrounds")
            "characters", (_characters, "Assets/Characters")
        ]
    
    
    
    do
        this.InitializeComponent()
        this.StretchWindowResolution()
        this.StretchBackgroundImage()
        this.SetBackground("bg1.jpg")
        this.SetCharacter("char1.png", "left")

    
    
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
        
    
    // Get Bitmap from List of paths
    member this.GetBitmap(key: string, filename: string) =
        match paths.TryFind key with
        | Some (dict, basePath) ->
            match dict.TryGetValue(filename) with
            | true, bitmap -> bitmap
            | false, _ ->
                let uri: Uri = Uri($"avares://Noulla/{basePath}/{filename}")
                let bitmap = new Bitmap(AssetLoader.Open(uri))
                dict[filename] <- bitmap
                bitmap
        | None ->
            failwithf $"Unknown key: {key}"
            

    // Set Background from Dictionary
    member this.SetBackground(background: string) =
        this.FindControl<Image>("Background").Source <- this.GetBitmap("backgrounds", background)
        
    
    // Set Character from Dictionary
    member this.SetCharacter(character: string, position: string) =
        match position with
        | "left" -> this.FindControl<Image>("CharacterLeftPosition").Source <- this.GetBitmap("characters", character)
        | "right" -> this.FindControl<Image>("CharacterRightPosition").Source <- this.GetBitmap("characters", character)
        | _ -> raise (ArgumentOutOfRangeException(nameof position, position, null))
        
    
    
    
    
    member private this.InitializeComponent() =
        #if DEBUG
        this.AttachDevTools()
        #endif
        AvaloniaXamlLoader.Load(this)