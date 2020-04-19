namespace Sanchez.Game.Platformer.Entity

open OpenToolkit.Mathematics
open Sanchez.Game.Platformer.Assets
open Sanchez.Game.Core
open FSharp.Data.UnitSystems.SI.UnitNames
open OpenToolkit.Graphics.OpenGL

type TextGameObject<'TTextureKey when 'TTextureKey : comparison>(name: string, id: int, shader, onUpdate, sqToFloat: float<sq> -> float32) =
    let conversion = Matrix4.CreateScale (1.<sq> |> sqToFloat)
    
    let mutable currentPosition = Position.create 0.<sq> 0.<sq>
    let mutable isAlive = true
    let mutable textTexture = None
    
    interface IGameObject<'TTextureKey> with
        member this.CurrentPosition with get() = currentPosition
        member this.IsAlive with get() = isAlive
        member this.Name with get() = name
        
        member this.Update (objectFinder, textureLoader, textLoader, timeElapsed) =
            let (alive, newPos, text) = onUpdate objectFinder currentPosition timeElapsed
            isAlive <- alive
            currentPosition <- newPos
            
            textTexture <- Some <| textLoader text
            
        member this.Render (widthScale) =
            Shader.useShader shader
            
            let widthTextScaling =
                match textTexture with
                | Some (TextTexture (str, x, widthTextScaling)) ->
                    GL.BindTexture(TextureTarget.Texture2D, x)
                    widthTextScaling
                | None ->
                    GL.BindTexture(TextureTarget.Texture2D, 0)
                    1.f
            GL.BindVertexArray id
            
            let transformLoc = Shader.getUniformLocation shader "transform"
            let pointScale = Matrix4.CreateScale(widthTextScaling / 2.f, 0.5f, 1.f)
            let trans = Matrix4.CreateTranslation(currentPosition.X |> float32, currentPosition.Y |> float32, 0.f)
            let scale = Matrix4.CreateScale (1.f / widthScale, 1.f, 1.f)
            let mat = pointScale * trans * scale * conversion
            GL.UniformMatrix4(transformLoc, false, ref mat)
            
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0)
            
    static member CreateTextGameObject name sqToFloat onUpdate shader =
        let vertexArrayId = GameObjectBase.createTexturePoints shader
        
        TextGameObject(name, vertexArrayId, shader, onUpdate, sqToFloat)
        
type TextGameObjectBinds =
    static member BindToObject<'TTextureKey when 'TTextureKey : comparison> parentName onUpdate =
        (fun (finder: string -> IGameObject<'TTextureKey> option) (lastPosition: Position) (elapsedTime: float<second>) ->
            match finder parentName with
            | Some x ->
                if x.IsAlive then
                    let (a, p, str) = onUpdate finder lastPosition elapsedTime
                    let parentedPosition = p + x.CurrentPosition
                    (a, parentedPosition, str)
                else (false, Position.create 0.<sq> 0.<sq>, "")
            | None -> (false, Position.create 0.<sq> 0.<sq>, ""))