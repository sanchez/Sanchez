namespace Sanchez.Game.Platformer.Entity

open FSharp.Data.UnitSystems.SI.UnitNames
open Sanchez.Game.Platformer.Assets
open Shader

type GameObjectManager<'TTextureKey when 'TTextureKey : comparison>(sqToFloat, textureLoader, textLoader) =
    let mutable aliveObjs: IGameObject<'TTextureKey> list = []
    
    member this.RemoveObject name =
        aliveObjs <- aliveObjs |> List.filter ((fun x -> x.Name = name) >> not)
    
    member this.FindGameObject name =
        aliveObjs
        |> List.tryFind (fun x -> x.Name = name)
        
    member this.LoadTexturedGameObject name onUpdate (shader: ShaderProgram) =
        let ob = TexturedGameObject.CreateTexturedGameObject name sqToFloat onUpdate shader
        aliveObjs <- aliveObjs @ [ob]
        
    member this.LoadTextGameObject name onUpdate (shader: ShaderProgram) =
        let ob = TextGameObject.CreateTextGameObject name sqToFloat onUpdate shader
        aliveObjs <- aliveObjs @ [ob]
    
    member this.AddGameObject ob =
        aliveObjs <- aliveObjs @ [ob]
        
    member this.Update (timeElapsed: float<second>) =
        aliveObjs <-
            aliveObjs
            |> List.filter (fun x ->
                x.Update(this.FindGameObject, textureLoader, textLoader, timeElapsed)
                x.IsAlive)
        
        ()
        
    member this.Render widthScale =
        aliveObjs
        |> List.iter (fun x -> x.Render(widthScale))