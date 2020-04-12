namespace Sanchez.Game.Platformer.Entity

open FSharp.Data.UnitSystems.SI.UnitNames
open Sanchez.Game.Platformer.Assets
open Shader

type GameObjectManager () =
    let mutable aliveObjs: GameObject list = []
    
    member this.LoadGameObject (tex: LoadedTexture) (shader: ShaderProgram) =
        let ob = GameObject.CreateTexturedGameObject shader tex
        aliveObjs <- aliveObjs @ [ob]
        
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