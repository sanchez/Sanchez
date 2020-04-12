module Sanchez.Game.Platformer.Entity.Shader

open System.IO
open OpenToolkit.Graphics.OpenGL

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