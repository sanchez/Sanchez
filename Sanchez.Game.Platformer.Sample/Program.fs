open Sanchez.Game.Platformer
open Sanchez.Game.Core

type Textures =
    | BodyTexture

[<EntryPoint>]
let main argv =
    let manager = new GameManager<Textures>("Hello World", 800, 600)
    
    manager.LoadTexture(BodyTexture, "Assets/body.png", (12, 12. * 1.<FPS>))
    
    manager.Run()
    printfn "Hello World from F#!"
    0 // return an integer exit code
