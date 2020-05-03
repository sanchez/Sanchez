namespace Sanchez.ThreeWin

open System.IO
open OpenToolkit.Graphics.OpenGL

type ShaderProgram = ShaderProgram of int

module Shaders =
    let useShader (ShaderProgram sPid) = GL.UseProgram sPid
    let getUniformLocation (ShaderProgram sPid) name = GL.GetUniformLocation(sPid, name)
    let getAttributeLocation (ShaderProgram sPid) name = GL.GetAttribLocation(sPid, name)
    
    let createSimpleShader (vertScript: string) (fragScript: string) =
        let vertexShader = GL.CreateShader ShaderType.VertexShader
        GL.ShaderSource(vertexShader, vertScript)
        GL.CompileShader vertexShader
        
        GL.GetShaderInfoLog vertexShader |> printf "**Vertex Shader Log:** \n%s"
        
        let fragmentShader = GL.CreateShader ShaderType.FragmentShader
        GL.ShaderSource(fragmentShader, fragScript)
        GL.CompileShader fragmentShader
        
        GL.GetShaderInfoLog fragmentShader |> printf "**Fragment Shader Log:** \n%s"
        
        let shaderProgram = GL.CreateProgram()
        GL.AttachShader(shaderProgram, vertexShader)
        GL.AttachShader(shaderProgram, fragmentShader)
        GL.LinkProgram shaderProgram
        
        GL.GetProgramInfoLog shaderProgram |> printf "**Program Linking Log:**\n%s"
        
        GL.DeleteShader vertexShader
        GL.DeleteShader fragmentShader
        
        printfn "Successfully loaded shaders"
        
        ShaderProgram shaderProgram
        
    let loadStandardShaders (shaderDir: string) =
        let keyVertLookup k = Path.Combine(shaderDir, "Shaders", sprintf "%s.vert" k)
        let keyFragLookup k = Path.Combine(shaderDir, "Shaders", sprintf "%s.frag" k)
        
        let loadVertFragPair (key: string) =
            let vertShader = key |> keyVertLookup |> File.ReadAllText
            let fragShader = key |> keyFragLookup |> File.ReadAllText
            createSimpleShader vertShader fragShader
        
        let simpleColor = loadVertFragPair "simpleColor"
        
        Map.empty
        |> Map.add "simpleColor" simpleColor