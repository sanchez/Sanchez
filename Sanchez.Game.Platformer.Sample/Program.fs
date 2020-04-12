// Learn more about F# at http://fsharp.org

open Sanchez.Game.Platformer
open System

[<EntryPoint>]
let main argv =
    let manager = new GameManager("Hello World", 800, 600)
    
    
    manager.Run()
    printfn "Hello World from F#!"
    0 // return an integer exit code
