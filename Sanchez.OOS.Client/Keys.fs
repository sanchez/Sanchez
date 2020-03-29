module Sanchez.OOS.Client.Keys

open OpenToolkit.Windowing.Common

type Key =
    | Escape = 0
    
    | Left = 1
    | Right = 2
    | Up = 3
    | Down = 4
    
    | Other = -1
    
let mapWindowKeyToKey (key: Input.Key) =
    match key with
    | Input.Key.A -> Key.Left
    | Input.Key.D -> Key.Right
    | Input.Key.W -> Key.Up
    | Input.Key.S -> Key.Down
    | Input.Key.Escape -> Key.Escape
    | _ -> Key.Other
    
let (|ParseGameEvent|) (evt: KeyboardKeyEventArgs) =
    (mapWindowKeyToKey evt.Key, evt.IsRepeat)