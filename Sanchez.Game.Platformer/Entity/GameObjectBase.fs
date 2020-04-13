module Sanchez.Game.Platformer.Entity.GameObjectBase

open OpenToolkit.Graphics.OpenGL

let createTexturePoints shader =
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
    
    vertexArrayId