namespace Sanchez.Game.Platformer.Entity

open OpenToolkit.Graphics.OpenGL
open Sanchez.Game.Platformer.Assets
open FSharp.Data.UnitSystems.SI.UnitNames
open OpenToolkit.Mathematics
open Sanchez.Game.Core

type GameObject<'TTextureKey when 'TTextureKey : comparison>(name: string, id: int, shader, onUpdate, sqToFloat: float<sq> -> float32) =
    let conversion = Matrix4.CreateScale(1.<sq> |> sqToFloat)
    let mutable nextTexture = None
    let mutable texDuration = 0.<second>
    
    let fetchTextureFrame () =
        match nextTexture with
        | Some x ->
            match x with
            | AnimatedTexture (frames, fps) ->
                let frame = fps * texDuration
                let wholeFrame =
                    frame |> int
                frames.[wholeFrame % frames.Length] |> Some
            | StaticTexture frame -> frame |> Some
        | None -> None
        
    member val CurrentPosition = Position.create 0.<sq> 0.<sq> with get, set
    member val FrameIteration = 0 with get, set
    member val IsAlive = true with get, set
    member val Name = name with get, set
    
    abstract member Update : (string -> GameObject<'TTextureKey> option) * ('TTextureKey -> LoadedTexture option) * (float<second>) -> unit
    default this.Update (objectFinder: string -> GameObject<'TTextureKey> option, textureLoader: 'TTextureKey -> LoadedTexture option, timeElapsed: float<second>) =
        let (isAlive, newPos, nextTex) = onUpdate this.CurrentPosition timeElapsed
        this.IsAlive <- isAlive
        this.CurrentPosition <- newPos
        
        let newTexture = textureLoader nextTex
        if newTexture = nextTexture then
            texDuration <- texDuration + timeElapsed
        else
            texDuration <- 0.<second>
            nextTexture <- newTexture
    
    member this.Render (widthScale: float32) =
        Shader.useShader shader
        match fetchTextureFrame() with
        | Some x -> GL.BindTexture(TextureTarget.Texture2D, x)
        | None -> GL.BindTexture(TextureTarget.Texture2D, 0)
        GL.BindVertexArray id
        
        let transformLoc = Shader.getUniformLocation shader "transform"
        let trans = Matrix4.CreateTranslation(this.CurrentPosition.X |> float32, this.CurrentPosition.Y |> float32, 0.f)
        let scale = Matrix4.CreateScale(1.f / widthScale, 1.f, 1.f)
        let mat = trans * scale * conversion
        GL.UniformMatrix4(transformLoc, false, ref mat)
        
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0)
    
    
    static member CreateTexturedGameObject name sqToFloat onUpdate shader =
        let vertexArrayId = GameObjectBase.createTexturePoints shader
        
        GameObject(name, vertexArrayId, shader, onUpdate, sqToFloat)
    
