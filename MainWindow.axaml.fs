namespace Noulla

open Noulla.modules
open Parser
open System
open System.IO
open Avalonia
open Avalonia.Controls
open Avalonia.Interactivity
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
    
    let mutable story: Story = Map.empty
    let mutable currentCommands: Command list = []
    let mutable currentIndex = 0
    
    let loadStoryJson() =
        let uri: Uri = Uri("avares://Noulla/Content/story.json")
        use stream = AssetLoader.Open(uri)
        use reader = new StreamReader(stream)
        reader.ReadToEnd()
    
    
    
    
    do
        this.InitializeComponent()
        this.StretchWindowResolution()
        this.StretchBackgroundImage()
        this.SetCharacter("char1.png", "left")
        
        
        //this.SetBackground(this.GetBitmap("backgrounds", "bg1.jpg"))
        
        //this.ClearCharacter("left")
        //this.SetTitleText("TEST")
        //this.SetBodyText("TEST")

        //let allButtons = ["left"; "center"; "right"]
        //allButtons |> List.iter (fun f -> this.SetButtonText("TEST", f))
    
        this.LoadStory()
    
    
    
    
    
    
    member this.LoadStory() =
        let json = loadStoryJson()
        story <- Parse(json)
        currentCommands <- story.["start"]
        currentIndex <- 0
        this.ExecuteNextCommand()
    
    member private this.ExecuteNextCommand() =
        if currentIndex < currentCommands.Length then
            
            let cmd = currentCommands.[currentIndex]
            
            match cmd with
            | Background img ->
                let bitmap = this.GetBitmap("backgrounds", img)
                this.SetBackground(bitmap)
            | CharacterShow (one, two) ->
                printfn "character show"
            | CharacterClear pos ->
                printfn "character clear"
            | TitleShow txt ->
                printfn "text"
            | TitleClear ->
                printfn "text"
            | TextShow txt ->
                this.SetTitleText("tst")
            | TextClear ->
                printfn "text"
            | ChoiceShow (id, txt, save, next) ->
                printfn "txt"
            | ChoiceHide id ->
                printfn "txt"
            | WaitClick ->
                ()
            | WaitChoice ->
                ()
            | Goto sceneId ->
                currentCommands <- story.[sceneId]
                currentIndex <- 0
                this.ExecuteNextCommand()
            | Save data ->
                printfn "data"

            match cmd with
            | WaitClick | WaitChoice -> ()
            | _ -> currentIndex <- currentIndex + 1
    
    
    
    
    
    // Get Bitmap from Dictionary
    member private this.GetBitmap(key: string, filename: string) =
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
    
    
    
    

    member this.TextClick(sender: obj, e: Avalonia.Input.PointerPressedEventArgs) =
        this.ClearCharacter("left")
        this.ClearCharacter("center")
        this.ClearCharacter("right")
        e.Handled <- true
        
    member this.ButtonChoiceLeft(sender: obj, e: RoutedEventArgs) =
        this.SetCharacter("char1.png", "left")
    
    member this.ButtonChoiceCenter(sender: obj, e: RoutedEventArgs) =
        this.SetCharacter("char1.png", "center")
    
    member this.ButtonChoiceRight(sender: obj, e: RoutedEventArgs) =
        this.SetCharacter("char1.png", "right")
    
    
    
    
    
    
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
        
    // Stretch & center Background image to FullScreen
    member private this.StretchBackgroundImage() =
        let width, height =
            match this.Screens.Primary with
            | null -> 0.0, 0.0
            | b -> b.Bounds.Width, b.Bounds.Height
        this.FindControl<Image>("Background").Width <- width
        this.FindControl<Image>("Background").Height <- height
        
    // Set Background from Dictionary
    member private this.SetBackground(bitmap: Bitmap) =
        this.FindControl<Image>("Background").Source <- bitmap
        
    // Set Character from Dictionary
    member private this.SetCharacter(character: string, position: string) =
        match position with
        | "left" -> this.FindControl<Image>("CharacterLeftPosition").Source <- this.GetBitmap("characters", character)
        | "right" -> this.FindControl<Image>("CharacterRightPosition").Source <- this.GetBitmap("characters", character)
        | "center" -> this.FindControl<Image>("CharacterCenterPosition").Source <- this.GetBitmap("characters", character)
        | _ -> raise (ArgumentOutOfRangeException(nameof position, position, null))
        
    // Clear Character
    member private this.ClearCharacter(position: string) =
        match position with
        | "left" -> this.FindControl<Image>("CharacterLeftPosition").Source <- null
        | "right" -> this.FindControl<Image>("CharacterRightPosition").Source <- null
        | "center" -> this.FindControl<Image>("CharacterCenterPosition").Source <- null
        | _ -> raise (ArgumentOutOfRangeException(nameof position, position, null))
        
    // Set Title Text
    member private this.SetTitleText(text: string) =
        this.FindControl<TextBlock>("Title").Text <- text
        
    // Set Body Text
    member private this.SetBodyText(text: string) =
        this.FindControl<TextBlock>("Body").Text <- text
        
    // Set Button Text
    member private this.SetButtonText(text: string, position: string) =
        match position with
        | "left" -> this.FindControl<Button>("Choice1").Content <- text
        | "center" -> this.FindControl<Button>("Choice2").Content <- text
        | "right" -> this.FindControl<Button>("Choice3").Content <- text
        | _ -> raise (ArgumentOutOfRangeException(nameof position, position, null))
            
            
    
    member private this.InitializeComponent() =
        #if DEBUG
        this.AttachDevTools()
        #endif
        AvaloniaXamlLoader.Load(this)