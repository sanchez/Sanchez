﻿open Sanchez.Game.Platformer
open Sanchez.Game.Core

type Textures =
    | BodyTexture

[<EntryPoint>]
let main argv =
    let manager = new GameManager<Textures>("Hello World", 800, 600)
    
    let playerUpdate position timeElapsed =
        let newPos = position + (Position.create 0.1<sq> 0.<sq>)
        (true, newPos)
    
    manager.LoadTexture(BodyTexture, "Assets/body.png", (12, 12. * 1.<FPS>))
    manager.LoadGameObject playerUpdate BodyTexture
    
    manager.Run()
    printfn "Hello World from F#!"
    0 // return an integer exit code
