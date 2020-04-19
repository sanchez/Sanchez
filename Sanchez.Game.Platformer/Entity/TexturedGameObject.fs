namespace Sanchez.Game.Platformer.Entity

open OpenToolkit.Mathematics
open Sanchez.Game.Platformer.Assets
open Sanchez.Game.Core
open FSharp.Data.UnitSystems.SI.UnitNames
open OpenToolkit.Graphics.OpenGL

type TexturedGameObject<'TTextureKey when 'TTextureKey : comparison>(name: string, id: int, shader, onUpdate, sqToFloat: float<sq> -> float32) =
    let conversion = Matrix4.CreateScale (1.<sq> |> sqToFloat)
    
    let mutable nextTexture = None
    let mutable texDuration = 0.<second>
    
    let fetchTextureFrame () =
        nextTexture
        |> Option.map (function
            | AnimatedTexture (frames, fps) ->
                let frame = fps * texDuration
                let wholeFrame = frame |> int
                frames.[wholeFrame % frames.Length]
            | StaticTexture frame -> frame)
        
    let mutable currentPosition = Position.create 0.<sq> 0.<sq>
    let mutable isAlive = true
    
    interface IGameObject<'TTextureKey> with
        member this.CurrentPosition with get() = currentPosition
        member this.IsAlive with get() = isAlive
        member this.Name with get() = name
            
        member this.Update (objectFinder, textureLoader, textLoader, timeElapsed) =
            let (alive, newPos, nextTex) = onUpdate objectFinder currentPosition timeElapsed
            isAlive <- alive
            currentPosition <- newPos
            
            let newTexture = textureLoader nextTex
            if newTexture = nextTexture then
                texDuration <- texDuration + timeElapsed
            else
                texDuration <- 0.<second>
                nextTexture <- newTexture
            
        member this.Render (widthScale) =
            Shader.useShader shader
            match fetchTextureFrame() with
            | Some x -> GL.BindTexture(TextureTarget.Texture2D, x)
            | None -> GL.BindTexture(TextureTarget.Texture2D, 0)
            GL.BindVertexArray id
            
            let transformLoc = Shader.getUniformLocation shader "transform"
            let trans = Matrix4.CreateTranslation(currentPosition.X |> float32, currentPosition.Y |> float32, 0.f)
            let scale = Matrix4.CreateScale(1.f / widthScale, 1.f, 1.f)
            let mat = trans * scale * conversion
            GL.UniformMatrix4(transformLoc, false, ref mat)
            
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0)
            
    static member CreateTexturedGameObject name sqToFloat onUpdate shader =
        let vertexArrayId = GameObjectBase.createTexturePoints shader
        
        TexturedGameObject(name, vertexArrayId, shader, onUpdate, sqToFloat)
        
type TexturedGameObjectBinds =
    static member BindToObject<'TTextureKey when 'TTextureKey : comparison> parentName defaultTexture onUpdate =
        (fun (finder: string -> IGameObject<'TTextureKey> option) (lastPosition: Position) (elapsedTime: float<second>) ->
            match finder parentName with
            | Some x ->
                if x.IsAlive then
                    let (a, p, t: 'TTextureKey) = onUpdate finder lastPosition elapsedTime
                    let parentedPosition = p + x.CurrentPosition
                    (a, parentedPosition, t)
                else (false, Position.create 0.<sq> 0.<sq>, defaultTexture)
            | None -> (false, Position.create 0.<sq> 0.<sq>, defaultTexture))