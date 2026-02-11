module Noulla.modules.Parser

open System.Text.Json
open Avalonia.Data


type ParsedCommand = {
    cmd: string
    img: string option
    name: string option
    position: string option
    text: string option
    id: string option
    save: string option
    next: string option
    scene: string option
    data: string option
    key: string option
    value: string option
}

type Command =
    | Background of img: string
    | CharacterShow of name: string * position: string
    | CharacterClear of position: string
    | TitleShow of text: string
    | TitleClear
    | TextShow of text: string
    | TextClear
    | ChoiceShow of id: string * text: string * save: string option * next: string option
    | ChoiceHide of id: string
    | WaitClick
    | WaitChoice
    | Goto of scene: string
    | Save of data: string
    | IfSave of key: string * expected: string * scene: string
    
let private toCommand (pc: ParsedCommand) : Command =
    match pc.cmd with
    | "background" -> Background pc.img.Value
    | "character_show" -> CharacterShow(pc.name.Value, pc.position.Value)
    | "character_clear" -> CharacterClear pc.position.Value
    | "title_show" -> TitleShow pc.text.Value
    | "title_clear" -> TitleClear
    | "text_show" -> TextShow pc.text.Value
    | "text_clear" -> TextClear
    | "choice_show" -> ChoiceShow(pc.id.Value, pc.text.Value, pc.save, pc.next)
    | "choice_hide" -> ChoiceHide pc.id.Value
    | "wait_click" -> WaitClick
    | "wait_choice" -> WaitChoice
    | "goto" -> Goto pc.scene.Value
    | "save" -> Save pc.data.Value
    | "if_save" -> IfSave(pc.key.Value, pc.value.Value, pc.next.Value)
    | _ -> failwith $"Unknown command: {pc.cmd}"



type Story = Map<string, Command list>

let Parse (jsonText: string) : Story =
    let root = JsonSerializer.Deserialize<{| scenes: Map<string, ParsedCommand array> |}>(jsonText)
    root.scenes |> Map.map (fun _ cmds -> cmds |> Array.map toCommand |> List.ofArray)