// Learn more about F# at http://fsharp.org

open System.Drawing
open System.Threading
open OpenToolkit.Mathematics
open Sanchez.Data.Positional
open Sanchez.ThreeWin

[<EntryPoint>]
let main argv =
    let cTokenSource = new CancellationTokenSource()
    let cToken = cTokenSource.Token
    
    let (winLoader, runner) = ThreeWin.createWindow "testing" 800 600 Color.White cToken
    
    let executionThread =
        async {
            do! Async.SwitchToNewThread()
            
            use! win = winLoader
            
            let! shaders = Shaders.loadStandardShaders ""
            
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
                
            let square = Vertexor.createColoredObject shaders squareColorizer squareVectors squareIndices
            
            win.SetOnRender (fun _ ->
                Vertexor.renderVertexor square Matrix4.Identity)
            
            return ()
        }
    Async.Start(executionThread, cToken)
    
    
    runner()
    cTokenSource.Cancel()
    0 // return an integer exit code
