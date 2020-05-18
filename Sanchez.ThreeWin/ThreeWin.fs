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
open Sanchez.Data.Positional

type ThreeWinKeyState =
    | KeyPressed
    | KeyReleasing
    | KeyReleased
    
type ThreeWinMouseButton =
    | MouseButtonLeft
    | MouseButtonRight
    | MouseButtonMiddle

type ThreeWin<'TKey when 'TKey : comparison>(title, width, height, clearColor: Color) =
    let windowSettings = GameWindowSettings.Default
    do windowSettings.RenderFrequency <- 60.
    do windowSettings.UpdateFrequency <- 60.
    
    let nativeSettings = NativeWindowSettings.Default
    do nativeSettings.Flags <- ContextFlags.ForwardCompatible
    do nativeSettings.Size <- Vector2i(width, height)
    do nativeSettings.Title <- title
    
    let mutable keyMap = []
    
    let gw = new GameWindow(windowSettings, nativeSettings)
    
    let mutable userLoadCB = fun () -> ()
    let onLoad () =
        GL.LoadBindings(new GLFWBindingsContext())
        GL.Enable(EnableCap.Blend)
        GL.Enable(EnableCap.DepthTest)
        GL.Enable(EnableCap.VertexProgramPointSize)
        GL.Enable(EnableCap.StencilTest)
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha)
        GL.ClearColor(clearColor)
        userLoadCB()
    do gw.add_Load(Action(onLoad))
    
    let onResize (args: ResizeEventArgs) =
        gw.Size <- Vector2i(width, height)
        // GL.Viewport(0, 0, width, height)
    do gw.add_Resize(Action<ResizeEventArgs>(onResize))
    
    let mutable currentKeyState = Map.empty
    
    let mutable mouseWheelDelta = 0.f
    do gw.add_MouseWheel(fun x ->
        mouseWheelDelta <- mouseWheelDelta + x.OffsetY)
    
    let mutable userUpdateCB = fun (timeSince: float<second>) -> ()
    let frameTimer = new Stopwatch()
    do frameTimer.Start()
    let onUpdate (args: FrameEventArgs) =
        gw.ProcessEvents() |> ignore
        
        currentKeyState <-
            keyMap
            |> Seq.map (fun (key, input) ->
                match gw.IsKeyDown input with
                | true -> (key, KeyPressed)
                | false -> (key, KeyReleasing))
            |> Seq.fold (fun m (key, state) ->
                let oldState = currentKeyState |> Map.tryFind key |> Option.defaultValue KeyReleased
                match (oldState, state) with
                | (_, KeyPressed) -> m |> Map.add key KeyPressed
                | (KeyPressed, KeyReleasing) -> m |> Map.add key KeyReleasing
                | (_, KeyReleasing) -> m |> Map.add key KeyReleased
                | _ -> m |> Map.add key KeyReleased
                ) currentKeyState
                
        let elapsed = frameTimer.ElapsedTicks
        frameTimer.Restart()
        let timeSince = ((elapsed |> float) / (Stopwatch.Frequency |> float)) * (1.<second>)
        userUpdateCB(timeSince)
        
        mouseWheelDelta <- 0.f
    do gw.add_UpdateFrame(Action<FrameEventArgs>(onUpdate))
    
    let getWidthScale () =
        let s = gw.Size
        (s.X |> float32) / (s.Y |> float32)
    
    let mutable userBackgroundRender = fun (widthScale: float32) -> ()
    let mutable userRenderCB = fun (widthScale: float32) -> ()
    let onRender (args: FrameEventArgs) =
        let widthScale = getWidthScale()
        GL.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit ||| ClearBufferMask.StencilBufferBit)
        GL.Disable(EnableCap.DepthTest)
        widthScale |> userBackgroundRender
        GL.Enable(EnableCap.DepthTest)
        
        widthScale |> userRenderCB
        
        gw.SwapBuffers()
    do gw.add_RenderFrame(Action<FrameEventArgs>(onRender))
    
    member this.SetOnLoad cb =
        userLoadCB <- cb
    member this.SetOnUpdate cb =
        userUpdateCB <- cb
    member this.SetOnRender cb =
        userRenderCB <- cb
    
    member this.AddKeyBinding (key: 'TKey) (gameKey: string) =
        keyMap <- (key, Input.Key.Parse(gameKey))::keyMap
    member this.RemoveKeyBinding (key: 'TKey) =
        keyMap <- keyMap |> List.filter (fst >> ((=) key) >> not)
    member this.WasKeyReleased (key: 'TKey) =
        currentKeyState
        |> Map.tryFind key
        |> Option.defaultValue KeyReleased
        |> function
            | KeyReleasing -> true
            | _ -> false
    member this.IsKeyDown (key: 'TKey) =
        currentKeyState
        |> Map.tryFind key
        |> Option.defaultValue KeyReleased
        |> function
            | KeyPressed -> true
            | _ -> false
            
    member this.GetWindowWidthScale () = getWidthScale()
    member this.GetWindowDimensions () =
        let sw = gw.Size
        (sw.X |> float, sw.Y |> float)
    
    member this.GetMousePosition () =
        let pos = gw.MousePosition
        let width = gw.Size.X |> float32
        let height = gw.Size.Y |> float32
        let scaledPos = PointVector.create (2.f * pos.X / width) (2.f * (1.f - pos.Y / height))
        scaledPos - (PointVector.create 1.f 1.f)
    member this.IsMouseButtonDown (btn: ThreeWinMouseButton) =
        match btn with
        | MouseButtonLeft -> Input.MouseButton.Left
        | MouseButtonRight -> Input.MouseButton.Right
        | MouseButtonMiddle -> Input.MouseButton.Middle
        |> gw.IsMouseButtonDown
    member this.GetMouseScroll () =
        mouseWheelDelta
        
    member this.SetBackgroundRender backing =
        userBackgroundRender <- backing
    
    member this.Run() = gw.Run()
    
    interface IDisposable with
        member this.Dispose() = gw.Dispose()
        
    static member createWindow<'TKey when 'TKey : comparison> title width height clearColor cToken =
        let win = new ThreeWin<'TKey>(title, width, height, clearColor)
        win