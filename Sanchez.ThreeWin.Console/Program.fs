// Learn more about F# at http://fsharp.org

open System
open System.Drawing
open System.Threading
open OpenToolkit.Mathematics
open Sanchez.Data.Positional
open Sanchez.Data.Positional
open Sanchez.ThreeWin

let loadSquare () =
    let shaders = Shaders.loadStandardShaders ""
    
    let squareVectors =
        [
            Vector.create -0.5f -0.5f 0.f
            Vector.create 0.5f -0.5f 0.f
            Vector.create 0.5f 0.5f 0.f
            Vector.create -0.5f 0.5f 0.f
        ]
    let squareIndices =
        [
            (0, 1, 2)
            (0, 3, 2)
        ]
    let squareColorizer _ =
        Color.Red
        
    Vertexor.createColoredObject shaders squareColorizer squareVectors squareIndices
    
let loadCube () =
    let shaders = Shaders.loadStandardShaders ""
    
    let squareVectors =
        [
            Vector.create -0.5f -0.5f 0.f   // front top left
            Vector.create 0.5f -0.5f 0.f    // front top right
            Vector.create 0.5f 0.5f 0.f     // front bottom right
            Vector.create -0.5f 0.5f 0.f    // front bottom left
            
            Vector.create -0.5f -0.5f 1.f   // back top left
            Vector.create 0.5f -0.5f 1.f    // back top right
            Vector.create 0.5f 0.5f 1.f     // back bottom right
            Vector.create -0.5f 0.5f 1.f    // back bottom left
        ]
    let squareIndices =
        [
            (0, 1, 2); (0, 3, 2)   // front face
            (0, 3, 4); (4, 7, 3)   // left face
            (1, 2, 6); (6, 5, 1)   // right face
        ]
    let squareColorizer (v: Vector<_>) =
        if v.Z = 1.f then Color.Blue
        else Color.Green
        
    Vertexor.createColoredObject shaders squareColorizer squareVectors squareIndices

[<EntryPoint>]
let main argv =
    let cTokenSource = new CancellationTokenSource()
    let cToken = cTokenSource.Token
    
    use win = ThreeWin.createWindow "testing" 800 600 Color.White cToken
    
    let camera = Vector.create 1.f 1.f 1.f |> Camera.create
    
    let mutable square = Vertexor.createEmpty()
    let mutable cube = Vertexor.createEmpty()
    
    win.SetOnLoad(fun () ->
        square <- loadSquare()
        cube <- loadCube()
        ())
    
    let mutable currentTimer = 0.
    win.SetOnUpdate(fun timeElapsed ->
        currentTimer <- currentTimer + (timeElapsed |> float)
        let x = (3. * Math.Sin currentTimer) |> float32
        let z = (3. * Math.Cos currentTimer) |> float32
        camera |> Camera.setPosition (Vector.create x 1.f z) |> ignore
        
        ())
    
    win.SetOnRender(fun widthScale ->
        let renderCam = Camera.renderCamera camera widthScale
        Matrix4.CreateTranslation(Vector3(1.f, 0.f, 0.f)) |> Vertexor.renderVertexor square renderCam
        
        Matrix4.Identity |> Vertexor.renderVertexor cube renderCam
        
        ())
    
    win.Run()
    cTokenSource.Cancel()
    0 // return an integer exit code
