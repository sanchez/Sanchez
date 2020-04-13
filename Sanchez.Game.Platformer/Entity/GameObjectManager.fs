namespace Sanchez.Game.Platformer.Entity

open FSharp.Data.UnitSystems.SI.UnitNames
open Sanchez.Game.Platformer.Assets
open Shader

type GameObjectManager<'TTextureKey when 'TTextureKey : comparison>(sqToFloat, texLoader) =
    let mutable aliveObjs: GameObject<'TTextureKey> list = []
    
    member this.FindGameObject name =
        aliveObjs
        |> List.tryFind (fun x -> x.Name = name)
    
    member this.LoadGameObject name onUpdate (shader: ShaderProgram) =
        let ob = GameObject.CreateTexturedGameObject name sqToFloat onUpdate shader
        aliveObjs <- aliveObjs @ [ob]
        
    member this.LoadParentedGameObject parentName name onUpdate shader =
        let ob = ParentedGameObject.CreateParentedTextureObject parentName name sqToFloat onUpdate shader
        aliveObjs <- aliveObjs @ [ob]
        
    member this.AddGameObject ob =
        aliveObjs <- aliveObjs @ [ob]
        
    member this.Update (timeElapsed: float<second>) =
        aliveObjs <-
            aliveObjs
            |> List.filter (fun x ->
                x.Update(this.FindGameObject, texLoader, timeElapsed)
                x.IsAlive)
        
        ()
        
    member this.Render widthScale =
        aliveObjs
        |> List.iter (fun x -> x.Render(widthScale))