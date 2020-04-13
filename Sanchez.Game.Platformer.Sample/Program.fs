open Sanchez.Game.Platformer
open Sanchez.Game.Core
open FSharp.Data.UnitSystems.SI.UnitNames

type Textures =
    | BodyTextureRight
    | BodyTextureLeft
    | StationaryTexture
    
type ActionKeys =
    | LeftKey
    | RightKey

[<EntryPoint>]
let main argv =
    let personSpeed = 15.<sq> / 1.<second>
    let fixedSquareNumber = 2.f / 15.f
    let squareUnitToPx (sq: float<sq>) = sq |> float32 |> ((*) fixedSquareNumber)
    
    let manager = new GameManager<Textures, ActionKeys>("Hello World", 800, 600, squareUnitToPx)
    manager.AddKeyBinding LeftKey "A"
    manager.AddKeyBinding RightKey "D"
    
    let playerUpdate position timeElapsed =
        let sideWaysPosition =
            match (manager.IsKeyPressed LeftKey, manager.IsKeyPressed RightKey) with
            | (true, true) -> Position.create 0.<sq> 0.<sq>
            | (true, _) -> Position.create -1.<sq> 0.<sq>
            | (_, true) -> Position.create 1.<sq> 0.<sq>
            | _ -> Position.create 0.<sq> 0.<sq>
        let distanceTravelled = personSpeed * timeElapsed
            
        let movementPos = ((sideWaysPosition) * distanceTravelled)
        
        let tex =
            if movementPos.X = 0.<sq> then StationaryTexture
            elif movementPos.X < 0.<sq> then BodyTextureLeft
            else BodyTextureRight
        
        (true, position + movementPos, tex)
    
    manager.LoadTexture(BodyTextureRight, "Assets/body.png", false, (12, 24.<frame/second>))
    manager.LoadTexture(BodyTextureLeft, "Assets/body.png", true, (12, 24.<frame/second>))
    manager.LoadTexture(StationaryTexture, "Assets/stationaryBody.png", false)
    manager.LoadGameObject playerUpdate
    
    manager.Run()
    printfn "Hello World from F#!"
    0 // return an integer exit code
