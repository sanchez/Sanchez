open Sanchez.Game.Platformer
open Sanchez.Game.Core

type Textures =
    | BodyTexture

[<EntryPoint>]
let main argv =
    let fixedSquareNumber = 2.f / 15.f
    let squareUnitToPx (sq: float<sq>) = sq |> float32 |> ((*) fixedSquareNumber)
    
    let manager = new GameManager<Textures>("Hello World", 800, 600, squareUnitToPx)
    
    let playerUpdate position timeElapsed =
//        let newPos = position + (Position.create 0.0001<sq> 0.<sq>)
        (true, position)
    
    manager.LoadTexture(BodyTexture, "Assets/body.png", (12, 12. * 1.<FPS>))
    manager.LoadGameObject playerUpdate BodyTexture
    
    manager.Run()
    printfn "Hello World from F#!"
    0 // return an integer exit code
