namespace Noulla

open System.Text.Json
open Avalonia.Controls.Templates
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



type ButtonData = {
    Text: string
    Save: string option
    Next: string option
}

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
    
    // In-memory save state
    let mutable privateSaveData: Map<string, string> = Map.empty
    
    // Get cross-platform save directory
    let getSaveDirectory() =
        let baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
        Path.Combine(baseDir, "Noulla", "saves")
    
    // Get path to save file
    let getSaveFile() =
        let dir = getSaveDirectory()
        Directory.CreateDirectory(dir) |> ignore
        Path.Combine(dir, "save.json")
    
    // Load save on start
    let loadSaveData() : Map<string, string> =
        let path = getSaveFile()
        if File.Exists(path) then
            try
                let json = File.ReadAllText(path)
                JsonSerializer.Deserialize<Map<string, string>>(json) |> Option.ofObj |> Option.defaultValue Map.empty
            with
            | _ ->
                printfn "WARN: Save file corrupted, starting fresh"
                Map.empty
        else
            Map.empty
            
    // Save on disk
    let saveSaveData(data: Map<string, string>) =
        try
            let path = getSaveFile()
            let options = JsonSerializerOptions(WriteIndented = true)
            let json = JsonSerializer.Serialize(data, options)
            File.WriteAllText(path, json)
        with
        | ex ->
            printfn $"ERROR saving: {ex.Message}"
    
    let mutable privateButtonData: Map<string, ButtonData> = Map.empty
    
    
    
    do
        this.InitializeComponent()
        privateSaveData <- loadSaveData()
        //this.StretchWindowResolution()
        this.StretchBackgroundImage()        
        //this.SetCharacter(this.GetBitmap("character", "char1.png"), "left")
        //this.SetBackground(this.GetBitmap("backgrounds", "bg1.jpg"))

        //let allButtons = ["left"; "center"; "right"]
        //allButtons |> List.iter (fun f -> this.SetButtonText("TEST", f))
    
        this.LoadStory()
    
    
    
    
    member this.LoadStory() =
        let json = loadStoryJson()
        story <- Parse(json)
        currentCommands <- story["start"]
        currentIndex <- 0
        this.ExecuteNextCommand()
    
    member private this.ExecuteNextCommand() =
        if currentIndex < currentCommands.Length then
            let cmd = currentCommands[currentIndex] // Get Command by Index and matching it
            
            match cmd with
            | WaitClick | WaitChoice ->
                ()
                
            | Goto sceneId ->
                currentCommands <- story[sceneId]
                currentIndex <- 0
                this.ExecuteNextCommand()
                
            | IfSave (key, expected, scene) ->
                match this.GetSaveValue(key) with
                | Some value when value = expected ->
                    currentCommands <- story[scene]
                    currentIndex <- 0
                    this.ExecuteNextCommand()
                | _ ->
                    currentIndex <- currentIndex + 1
                    this.ExecuteNextCommand()

            | _ ->
                match cmd with
                | Background img ->
                    let bitmap = this.GetBitmap("backgrounds", img)
                    this.SetBackground(bitmap)
                | CharacterShow (name, position) ->
                    let bitmap = this.GetBitmap("characters", name)
                    this.SetCharacter(bitmap, position)
                | CharacterClear position ->
                    this.ClearCharacter position
                | TitleShow text ->
                    this.SetTitleText text
                | TitleClear ->
                    this.ClearTitleText()
                | TextShow text ->
                    this.SetBodyText text
                | TextClear ->
                    this.ClearBodyText()
                | ChoiceShow (id, text, save, next) ->
                    privateButtonData <- privateButtonData |> Map.add id { Text = text; Save = save; Next = next }
                    this.SetChoice(text, id)
                | ChoiceHide id ->
                    privateButtonData <- privateButtonData |> Map.remove id
                    this.HideChoice id
                | Save data ->
                    this.HandleSave data
                | WaitClick | WaitChoice | Goto _ | IfSave _ -> ()

                currentIndex <- currentIndex + 1
                this.ExecuteNextCommand()
    
    
    
    // Get save value
    member this.GetSaveValue(key: string) : string option =
        Map.tryFind key privateSaveData 
    
    // Main save function
    member private this.HandleSave(data: string) =
        let parts = data.Split(' ', 2)
        if parts.Length = 2 then
            let key, value = parts[0], parts[1]
            privateSaveData <- privateSaveData |> Map.add key value
            saveSaveData privateSaveData
            printfn $"SAVE: {key} = {value}"
        else
            printfn $"WARN: Invalid save format: {data}"
    
    
    
    
    
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
    
    
    
    

    member this.TextClick(_sender: obj, _e: Avalonia.Input.PointerPressedEventArgs) =
         if currentIndex < currentCommands.Length then
             match currentCommands[currentIndex] with
             | WaitClick ->
                 currentIndex <- currentIndex + 1
                 this.ExecuteNextCommand()
             | _ -> ()
        
    member this.OnButtonClick(id: string) =
        if currentIndex >= currentCommands.Length then ()
        else
            match currentCommands[currentIndex] with
            | WaitChoice ->
                match Map.tryFind id privateButtonData with
                | Some data ->
                    data.Save |> Option.iter this.HandleSave
                    match data.Next with
                    | Some sceneId ->
                        currentCommands <- story[sceneId]
                        currentIndex <- 0
                    | None -> currentIndex <- currentIndex + 1
                    this.ExecuteNextCommand()
                | None -> printfn $"WARN: Button {id} has no data"
            | _ -> ()



    member this.OnButtonClickHandler(sender: obj, _e: RoutedEventArgs) =
        match sender with
        | :? Button as btn ->
            match btn.Name with
            | "Choice1" -> this.OnButtonClick("1")
            | "Choice2" -> this.OnButtonClick("2")
            | "Choice3" -> this.OnButtonClick("3")
            | name -> printfn $"WARN: Unknown button name: {name}"
        | _ -> ()
    
    
    
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
    member private this.SetBackground(background: Bitmap) =
        this.FindControl<Image>("Background").Source <- background
        
    // Set Character from Dictionary
    member private this.SetCharacter(character: Bitmap, position: string) =
        match position with
        | "left" -> this.FindControl<Image>("CharacterLeftPosition").Source <- character
        | "center" -> this.FindControl<Image>("CharacterCenterPosition").Source <- character
        | "right" -> this.FindControl<Image>("CharacterRightPosition").Source <- character
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
        
    // Clear Title
    member private this.ClearTitleText() =
        this.FindControl<TextBlock>("Title").Text <- null
                
    // Set Body Text
    member private this.SetBodyText(text: string) =
        this.FindControl<TextBlock>("Body").Text <- text
        
    // Clear Body Text
    member private this.ClearBodyText() =
        this.FindControl<TextBlock>("Body").Text <- null
            
    // Set ChoiceButton Text
    member private this.SetChoice(text: string, id: string) =
        match id with
        | "1" ->
            this.FindControl<Button>("Choice1").IsVisible <- true
            this.FindControl<Button>("Choice1").Content <- text
        | "2" ->
            this.FindControl<Button>("Choice2").IsVisible <- true
            this.FindControl<Button>("Choice2").Content <- text
        | "3" ->
            this.FindControl<Button>("Choice3").IsVisible <- true
            this.FindControl<Button>("Choice3").Content <- text
        | _ -> raise (ArgumentOutOfRangeException(nameof id, id, null))
    
    // Hide Choice Buttons by ID
    member private this.HideChoice(id: string) =
        match id with
        | "1" -> this.FindControl<Button>("Choice1").IsVisible <- false
        | "2" -> this.FindControl<Button>("Choice2").IsVisible <- false
        | "3" -> this.FindControl<Button>("Choice3").IsVisible <- false
        | _ -> raise (ArgumentOutOfRangeException(nameof id, id, null))
    
    
    
    
    
    member private this.InitializeComponent() =
        #if DEBUG
        this.AttachDevTools()
        #endif
        AvaloniaXamlLoader.Load(this)