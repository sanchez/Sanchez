namespace Sanchez.Game.Platformer.Entity

open FSharp.Data.UnitSystems.SI.UnitNames
open Sanchez.Game.Platformer.Assets
open Shader

type GameObjectManager<'TTextureKey when 'TTextureKey : comparison>(sqToFloat, texLoader) =
    let mutable aliveObjs: GameObject<'TTextureKey> list = []
    
    member this.LoadGameObject onUpdate (shader: ShaderProgram) =
        let ob = GameObject.CreateTexturedGameObject sqToFloat onUpdate shader
        aliveObjs <- aliveObjs @ [ob]
        
    member this.AddGameObject ob =
        aliveObjs <- aliveObjs @ [ob]
        
    member this.Update (timeElapsed: float<second>) =
        aliveObjs <-
            aliveObjs
            |> List.filter (fun x ->
                x.Update texLoader timeElapsed
                x.IsAlive)
        
        ()
        
    member this.Render widthScale =
        aliveObjs
        |> List.iter (fun x -> x.Render(widthScale))