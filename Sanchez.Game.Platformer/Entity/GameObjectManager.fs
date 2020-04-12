namespace Sanchez.Game.Platformer.Entity

open FSharp.Data.UnitSystems.SI.UnitNames

type GameObjectManager () =
    let mutable aliveObjs: GameObject list = []
    
    member this.LoadGameObject () =
        ()
        
    member this.Update (timeElapsed: float<second>) =
        aliveObjs <-
            aliveObjs
            |> List.filter (fun x ->
                x.Update(timeElapsed)
                x.IsAlive)
        
        ()
        
    member this.Render () =
        aliveObjs
        |> List.iter (fun x -> x.Render())