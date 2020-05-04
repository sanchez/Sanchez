namespace Sanchez.ThreeWin

open System
open System.Diagnostics
open System.Drawing
open OpenToolkit.Graphics.OpenGL
open OpenToolkit.Mathematics
open OpenToolkit.Windowing.Common
open OpenToolkit.Windowing.Desktop
open OpenToolkit.Windowing.GraphicsLibraryFramework
open FSharp.Data.UnitSystems.SI.UnitNames

type ThreeWin(title, width, height, clearColor: Color) =
    let windowSettings = GameWindowSettings.Default
    do windowSettings.RenderFrequency <- 60.
    do windowSettings.UpdateFrequency <- 60.
    
    let nativeSettings = NativeWindowSettings.Default
    do nativeSettings.Flags <- ContextFlags.ForwardCompatible
    do nativeSettings.Size <- Vector2i(width, height)
    do nativeSettings.Title <- title
    
    let gw = new GameWindow(windowSettings, nativeSettings)
    
    let mutable userLoadCB = fun () -> ()
    let onLoad () =
        GL.LoadBindings(new GLFWBindingsContext())
        GL.Enable(EnableCap.Blend)
        GL.Enable(EnableCap.DepthTest)
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha)
        GL.ClearColor(clearColor)
        userLoadCB()
    do gw.add_Load(Action(onLoad))
    
    let onResize (args: ResizeEventArgs) =
        gw.Size <- Vector2i(width, height)
        // GL.Viewport(0, 0, width, height)
    do gw.add_Resize(Action<ResizeEventArgs>(onResize))
    
    let mutable userUpdateCB = fun (timeSince: float<second>) -> ()
    let frameTimer = new Stopwatch()
    do frameTimer.Start()
    let onUpdate (args: FrameEventArgs) =
        gw.ProcessEvents() |> ignore
        
        let elapsed = frameTimer.ElapsedTicks
        frameTimer.Restart()
        let timeSince = ((elapsed |> float) / (Stopwatch.Frequency |> float)) * (1.<second>)
        userUpdateCB(timeSince)
    do gw.add_UpdateFrame(Action<FrameEventArgs>(onUpdate))
    
    let mutable userRenderCB = fun (widthScale: float32) -> ()
    let onRender (args: FrameEventArgs) =
        GL.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
        
        let s = gw.Size
        let adjustment = (s.X |> float32) / (s.Y |> float32)
        
        userRenderCB adjustment
        
        gw.SwapBuffers()
    do gw.add_RenderFrame(Action<FrameEventArgs>(onRender))
    
    member this.SetOnLoad cb =
        userLoadCB <- cb
    member this.SetOnUpdate cb =
        userUpdateCB <- cb
    member this.SetOnRender cb =
        userRenderCB <- cb
    
    member this.Run() = gw.Run()
    
    interface IDisposable with
        member this.Dispose() = gw.Dispose()
        
module ThreeWin =
    let createWindow title width height clearColor cToken =
        let win = new ThreeWin(title, width, height, clearColor)
//        let loadThread = Async.FromContinuations(fun (success, failed, _) ->
//            win.SetOnLoad(fun () -> success win))
//        
//        (loadThread, fun () -> win.Run())
        win