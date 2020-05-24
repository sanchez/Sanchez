namespace Sanchez.FunCAD

open System.Drawing
open System.Threading
open OpenToolkit.Mathematics
open Sanchez.Data.Positional
open Sanchez.ThreeWin

type FunCADKeys =
    | Escape

module FunCAD =
    let initialize title onLoad =
        let cTokenSource = new CancellationTokenSource()
        let cToken = cTokenSource.Token
        
        use win = ThreeWin.createWindow<FunCADKeys> title 800 600 Color.White cToken
        
        let (cameraUpdate, cameraRender, _) = ControlledOrbitalCamera.create 2. 4.f win
        
        let scene = Scene.create()
        
        let mutable background = Vertexor.createEmpty()
        let loadBackground shaders =
            let backgroundColorizer (v: PointVector<_>) =
                match v.Y with
                | -1.f -> Color.DarkSlateGray
                | 1.f -> Color.LightGray
                | _ -> Color.Red
            Vertexor.createStaticBackground shaders backgroundColorizer
        
        win.SetOnLoad(fun () ->
            let shaders = Shaders.loadStandardShaders ""
            
            background <- loadBackground shaders
            
            onLoad shaders scene
            
            ())
        
        win.SetOnUpdate(fun timeElapsed ->
            cameraUpdate timeElapsed)
        
        win.SetBackgroundRender(fun widthScale ->
            let renderCam = cameraRender widthScale
            Matrix4.Identity |> Vertexor.renderVertexor background renderCam)
        
        win.SetOnRender(fun widthScale ->
            let renderCam = cameraRender widthScale
            scene |> Scene.renderScene renderCam
            ())
        
        win.Run()
        cTokenSource.Cancel()
