// Learn more about F# at http://fsharp.org

open System.Drawing
open System.Threading
open OpenToolkit.Mathematics
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

[<EntryPoint>]
let main argv =
    let cTokenSource = new CancellationTokenSource()
    let cToken = cTokenSource.Token
    
    use win = ThreeWin.createWindow "testing" 800 600 Color.Orange cToken
    
    let mutable square = Vertexor.createEmpty()
    
    win.SetOnLoad(fun () ->
        square <- loadSquare()
        ())
    
    win.SetOnUpdate(fun timeElapsed ->
        ())
    
    win.SetOnRender(fun widthScale ->
        Vertexor.renderVertexor square Matrix4.Identity
        
        ())
    
    win.Run()
    cTokenSource.Cancel()
    0 // return an integer exit code
