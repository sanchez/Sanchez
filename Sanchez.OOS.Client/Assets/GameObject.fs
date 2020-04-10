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
    
    let fragmentShaderSource = File.ReadAllText("Assets/shader.frag")
    let fragmentShader = GL.CreateShader ShaderType.FragmentShader
    GL.ShaderSource(fragmentShader, fragmentShaderSource)
    GL.CompileShader(fragmentShader)
    
    let shaderProgram = GL.CreateProgram()
    GL.AttachShader(shaderProgram, vertexShader)
    GL.AttachShader(shaderProgram, fragmentShader)
    GL.LinkProgram shaderProgram
    
    ShaderProgram shaderProgram

type GameObject =
    {
        Texture: LoadedTexture option
    }
    
let loadObject (ShaderProgram sPid) =
    let points =
        [|
            -0.5; 0.; 0.
            0.5; 0.; 0.
            0.; 0.5; 0.
        |]
    
    let obId = GL.GenBuffer()
    GL.BindBuffer(BufferTarget.ArrayBuffer, obId)
    GL.BufferData(BufferTarget.ArrayBuffer, points.Length * sizeof<float>, points, BufferUsageHint.StaticDraw)
    
    let positionLocation = GL.GetAttribLocation(sPid, "position")
    
    let vArrayObj = GL.GenVertexArray()
    GL.BindVertexArray(vArrayObj)
    GL.VertexAttribPointer(positionLocation, 4, VertexAttribPointerType.Float, false, 0, 0)
    GL.EnableVertexAttribArray(positionLocation)
    
    
    ()