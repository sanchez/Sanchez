module Sanchez.OOS.Client.Assets.GameObject

open System.IO
open OpenToolkit.Graphics.OpenGL
open Sanchez.OOS.Client.Assets.Assets

type ShaderProgram = ShaderProgram of int

let loadShaders () =
    let vertexShaderSource = File.ReadAllText("Assets/shader.vert")
    let vertexShader = GL.CreateShader ShaderType.VertexShader
    GL.ShaderSource(vertexShader, vertexShaderSource)
    GL.CompileShader(vertexShader)
    
    GL.GetShaderInfoLog vertexShader |> printfn "**Vertex Shader Log:** \n%s"
    
    let fragmentShaderSource = File.ReadAllText("Assets/shader.frag")
    let fragmentShader = GL.CreateShader ShaderType.FragmentShader
    GL.ShaderSource(fragmentShader, fragmentShaderSource)
    GL.CompileShader(fragmentShader)
    
    GL.GetShaderInfoLog fragmentShader |> printfn "**Fragment Shader Log:**\n%s"
    
    let shaderProgram = GL.CreateProgram()
    GL.AttachShader(shaderProgram, vertexShader)
    GL.AttachShader(shaderProgram, fragmentShader)
    GL.LinkProgram shaderProgram
    
    GL.GetProgramInfoLog shaderProgram |> printfn "**Program Linking Log:**\n%s"
    
    GL.DeleteShader vertexShader
    GL.DeleteShader fragmentShader
    
    printfn "Successfully loaded shaders"
    
    ShaderProgram shaderProgram

type GameObject =
    {
        Id: int
//        Texture: LoadedTexture option
    }
    
let loadObject (ShaderProgram sPid) =
    let vertices =
        [|
            -0.5f; -0.5f; 0.f;   1.f; 0.f; 0.f; // bottom left
             0.5f; -0.5f; 0.f;   0.f; 1.f; 0.f; // bottom right
             0.5f;  0.5f; 0.f;   0.f; 0.f; 1.f; // top right
            -0.5f;  0.5f; 0.f;   1.f; 1.f; 1.f; // top left
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
    
    let positionLocation = GL.GetAttribLocation(sPid, "aPos")
    let colorLocation = GL.GetAttribLocation(sPid, "aColor")
    
    
    GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof<float32>, 0)
    GL.EnableVertexAttribArray(positionLocation)
    
    GL.VertexAttribPointer(colorLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof<float32>, 3 * sizeof<float32>)
    GL.EnableVertexAttribArray(colorLocation)
    
    {
        GameObject.Id = vertexArrayId
    }