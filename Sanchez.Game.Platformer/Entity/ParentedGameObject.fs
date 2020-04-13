namespace Sanchez.Game.Platformer.Entity

open OpenToolkit.Graphics.OpenGL
open Sanchez.Game.Platformer.Assets
open FSharp.Data.UnitSystems.SI.UnitNames
open OpenToolkit.Mathematics
open Sanchez.Game.Core

type ParentedGameObject<'TTextureKey when 'TTextureKey : comparison>(parentObj: string, name: string, id: int, shader, onUpdate, sqToFloat: float<sq> -> float32) =
    inherit GameObject<'TTextureKey>(name, id, shader, onUpdate, sqToFloat)
    
    let mutable parentPosition = Position.create 0.<sq> 0.<sq>
    
    override this.Update (objectFinder: string -> GameObject<'TTextureKey> option, textureLoader: 'TTextureKey -> LoadedTexture option, timeElapsed: float<second>) =
        base.Update(objectFinder, textureLoader, timeElapsed)
        match objectFinder parentObj with
        | Some x -> parentPosition <- x.CurrentPosition
        | None -> this.IsAlive <- false
        this.CurrentPosition <- this.CurrentPosition + parentPosition
        
    static member CreateParentedTextureObject parentName name sqToFloat onUpdate shader =
        let vertexArrayId = GameObjectBase.createTexturePoints shader
        
        ParentedGameObject(parentName, name, vertexArrayId, shader, onUpdate, sqToFloat)