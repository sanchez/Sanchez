namespace Sanchez.Game.Platformer.Entity

open OpenToolkit.Graphics.OpenGL
open Sanchez.Game.Platformer.Assets
open FSharp.Data.UnitSystems.SI.UnitNames
open OpenToolkit.Mathematics
open Sanchez.Game.Core

type GameObject(id: int, tex: LoadedTexture, shader, onUpdate, sqToFloat: float<sq> -> float32) =
    let conversion = Matrix4.CreateScale(1.<sq> |> sqToFloat)
    
    let fetchTextureFrame () =
        match tex with
        | AnimatedTexture (frames, fps) -> frames.[0]
        | StaticTexture frame -> frame
        
    let mutable currentPosition = Position.create 0.<sq> 0.<sq>
    
    member val FrameIteration = 0 with get, set
    member val IsAlive = true with get, set
    
    member this.Update(timeElapsed: float<second>) =
        let (isAlive, newPos) = onUpdate currentPosition timeElapsed
        this.IsAlive <- isAlive
        currentPosition <- newPos
    
    member this.Render (widthScale: float32) =
        Shader.useShader shader
        GL.BindTexture(TextureTarget.Texture2D, fetchTextureFrame())
        GL.BindVertexArray id
        
        let transformLoc = Shader.getUniformLocation shader "transform"
        let trans = Matrix4.CreateTranslation(currentPosition.X |> sqToFloat, currentPosition.Y |> sqToFloat, 1.f)
        let scale = Matrix4.CreateScale(1.f / widthScale, 1.f, 1.f)
        let mat = trans * scale * conversion
        GL.UniformMatrix4(transformLoc, false, ref mat)
//        GL.UniformMatrix4
        
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0)
    
    
    static member CreateTexturedGameObject sqToFloat onUpdate shader (tex: LoadedTexture) =
        let vertices =
            [|
                // positions         // texture coords
                -0.5f; -0.5f; 0.f;   0.f; 0.f;  // bottom left
                 0.5f; -0.5f; 0.f;   1.f; 0.f;  // bottom right
                 0.5f;  0.5f; 0.f;   1.f; 1.f;  // top right
                -0.5f;  0.5f; 0.f;   0.f; 1.f;  // top left
            |]
            
        let indices =
            [|
                3u; 0u; 1u
                3u; 2u; 1u
            |]
            
        let vertexArrayId = GL.GenVertexArray()
        GL.BindVertexArray vertexArrayId
        
        let vertexBufferId = GL.GenBuffer()
        let elementBufferId = GL.GenBuffer()
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId)
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof<float32>, vertices, BufferUsageHint.StaticDraw)
        
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferId)
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof<uint32>, indices, BufferUsageHint.StaticDraw)
        
        let positionLocation = Shader.getAttributeLocation shader "aPos"
        let texLocation = Shader.getAttributeLocation shader "aTexCoord"
        
        GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof<float32>, 0)
        GL.EnableVertexAttribArray(positionLocation)
        
        GL.VertexAttribPointer(texLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof<float32>, 3 * sizeof<float32>)
        GL.EnableVertexAttribArray(texLocation)
        
        GameObject(vertexArrayId, tex, shader, onUpdate, sqToFloat)
    
