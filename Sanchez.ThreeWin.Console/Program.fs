// Learn more about F# at http://fsharp.org

open System
open System.Drawing
open System.Threading
open OpenToolkit.Mathematics
open Sanchez.Data.Positional
open Sanchez.ThreeWin

let loadSquare shaders =
    let squareVectors =
        [
            Vector.create -5.f 0.f -5.f
            Vector.create 5.f 0.f -5.f
            Vector.create 5.f 0.f 5.f
            Vector.create -5.f 0.f 5.f
        ]
    let squareIndices =
        [
            (0, 1, 2)
            (0, 3, 2)
        ]
    let squareColorizer _ = Color.Gray
        
    Vertexor.createColoredObject shaders squareColorizer squareVectors squareIndices
    
let loadCube shaders =
    let squareVectors =
        [
            Vector.create -0.5f -0.5f -0.5f   // front bottom left
            Vector.create 0.5f -0.5f -0.5f    // front bottom right
            Vector.create 0.5f 0.5f -0.5f     // front top right
            Vector.create -0.5f 0.5f -0.5f    // front top left
            
            Vector.create -0.5f -0.5f 0.5f   // back bottom left
            Vector.create 0.5f -0.5f 0.5f    // back bottom right
            Vector.create 0.5f 0.5f 0.5f     // back top right
            Vector.create -0.5f 0.5f 0.5f    // back top left
        ]
    let squareIndices =
        [
            (0, 1, 2); (0, 3, 2)   // front face
            (0, 3, 4); (4, 7, 3)   // left face
            (1, 2, 6); (6, 5, 1)   // right face
            (4, 5, 6); (6, 7, 4)   // back face
            (0, 1, 4); (4, 5, 1)   // bottom face
            (2, 3, 7); (7, 6, 2)   // top face
        ]
    let squareColorizer (v: Vector<_>) =
        match (v.Z, v.Y) with
        | (0.5f, 0.5f) -> Color.Orange
        | (0.5f, _) -> Color.Purple
        | (_, 0.5f) -> Color.Green
        | _ -> Color.Blue
        
    Vertexor.createColoredObject shaders squareColorizer squareVectors squareIndices
    |> Vertexor.applyStaticTransformation (Matrix4.CreateTranslation(0.f, 0.5f, 0.f))
    
let loadPlayer shaders =
    let (leftHead, rightHead) = Textures.loadDoubleSidedStaticTexture "Assets/head1.png" |> Option.get
    let playerVectors =
        [
            (Vector.create -0.5f -0.5f 0.f, PointVector.create 0.f 0.f)  // bottom left
            (Vector.create 0.5f -0.5f 0.f, PointVector.create 1.f 0.f)   // bottom right
            (Vector.create 0.5f 0.5f 0.f, PointVector.create 1.f 1.f)    // top right
            (Vector.create -0.5f 0.5f 0.f, PointVector.create 0.f 1.f)   // top left
        ]
    let playerIndices =
        [
            (0, 1, 2)
            (0, 3, 2)
        ]
    
    let onHeadUpdate () = leftHead
        
    Vertexor.createTexturedObject shaders playerVectors playerIndices onHeadUpdate
    |> Vertexor.applyStaticTransformation (Matrix4.CreateTranslation(3.f, 0.5f, 0.f))

[<EntryPoint>]
let main argv =
    let cTokenSource = new CancellationTokenSource()
    let cToken = cTokenSource.Token
    
    use win = ThreeWin.createWindow "testing" 800 600 Color.White cToken
    
    let camera = Vector.create 1.f 1.f 1.f |> Camera.create
    
    let mutable square = Vertexor.createEmpty()
    let mutable cube = Vertexor.createEmpty()
    let mutable player = Vertexor.createEmpty()
    
    win.SetOnLoad(fun () ->
        let shaders = Shaders.loadStandardShaders ""
        
        square <- loadSquare shaders
        cube <- loadCube shaders
        player <- loadPlayer shaders
        ())
    
    let mutable currentTimer = 0.
    win.SetOnUpdate(fun timeElapsed ->
        currentTimer <- currentTimer + (timeElapsed |> float)
        let x = (4. * Math.Sin currentTimer) |> float32
        let z = (4. * Math.Cos currentTimer) |> float32
        camera |> Camera.setPosition (Vector.create x 4.f z) |> ignore
        
        ())
    
    win.SetOnRender(fun widthScale ->
        let renderCam = Camera.renderCamera camera widthScale
        
        Matrix4.Identity |> Vertexor.renderVertexor square renderCam
        
        Matrix4.Identity |> Vertexor.renderVertexor cube renderCam
        
        Matrix4.Identity |> Vertexor.renderVertexor player renderCam
        
        ())
    
    win.Run()
    cTokenSource.Cancel()
    0 // return an integer exit code
