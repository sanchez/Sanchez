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
            -0.5; -0.5; 0.
            0.5; -0.5; 0.
            0.; 0.5; 0.
        |]
        
    let vertexBufferId = GL.GenBuffer()
    GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId)
    GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof<float>, vertices, BufferUsageHint.StaticDraw)
    
    let positionLocation = GL.GetAttribLocation(sPid, "position")
    
    let vertexArrayId = GL.GenVertexArray()
    GL.BindVertexArray vertexArrayId
    GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Double, false, 3 * sizeof<float>, 0)
    GL.EnableVertexAttribArray(positionLocation)
    
    {
        GameObject.Id = vertexArrayId
    }