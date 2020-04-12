namespace Sanchez.Game.Platformer

open OpenToolkit.Graphics.OpenGL
open System
open System.Diagnostics
open System.Drawing
open OpenToolkit.Mathematics
open OpenToolkit.Windowing.Common
open OpenToolkit.Windowing.Desktop
open OpenToolkit.Windowing.GraphicsLibraryFramework
open FSharp.Data.UnitSystems.SI.UnitNames

type Game (title, width, height, loadCB, updateCB, renderCB) =
    let windowSettings = GameWindowSettings.Default
    do windowSettings.RenderFrequency <- 60.
    do windowSettings.UpdateFrequency <- 60.
    
    let nativeSettings = NativeWindowSettings.Default
    do nativeSettings.Size <- Vector2i(width, height)
    do nativeSettings.Title <- title 

    let gw = new GameWindow(windowSettings, nativeSettings)
    
    let onLoad () =
        GL.LoadBindings(new GLFWBindingsContext())
        GL.Enable(EnableCap.Blend)
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha)
        GL.ClearColor(Color.Black)
        loadCB()
    do gw.add_Load(Action(onLoad))
    
    let onResize (args: ResizeEventArgs) =
        gw.Size <- Vector2i(width, height)
        // GL.Viewport(0, 0, width, height)
    do gw.add_Resize(Action<ResizeEventArgs>(onResize))
    
    let frameTimer = new Stopwatch()
    do frameTimer.Start()
    let onUpdate (args: FrameEventArgs) =
        gw.ProcessEvents() |> ignore
        
        let elapsed = frameTimer.ElapsedTicks
        frameTimer.Restart()
        let timeSince = ((elapsed |> float) / (Stopwatch.Frequency |> float)) * (1.<second>)
        
        updateCB(timeSince)
        
        ()
    do gw.add_UpdateFrame(Action<FrameEventArgs>(onUpdate))
    
    let onRender (args: FrameEventArgs) =
        GL.Clear(ClearBufferMask.ColorBufferBit)
        
        let s = gw.Size
        let adjustment = (s.X |> float32) / (s.Y |> float32)
        
        renderCB adjustment
        
        gw.SwapBuffers()
    do gw.add_RenderFrame(Action<FrameEventArgs>(onRender))
    
    
    member this.Run() =
        gw.Run()
        
    interface IDisposable with
        member this.Dispose () =
            gw.Dispose()