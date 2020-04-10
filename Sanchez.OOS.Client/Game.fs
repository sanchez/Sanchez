namespace Sanchez.OOS.Client

open Sanchez.OOS.Client.Assets
open OpenToolkit.Mathematics
open OpenToolkit.Windowing.Common
open System
open System.Collections.Generic
open System.Diagnostics
open System.Drawing
open OpenToolkit.Windowing.Desktop
open Sanchez.OOS.Client.Keys
open Sanchez.OOS.Core.GameCore
open FSharp.Data.UnitSystems.SI.UnitNames
open OpenToolkit.Graphics.OpenGL
open Sanchez.OOS.Core

type Game (width, height, sender: ClientAction -> unit) =
    let windowSettings = GameWindowSettings.Default
    do windowSettings.RenderFrequency <- 60.0
    do windowSettings.UpdateFrequency <- 60.0
    
    let nativeSettings = NativeWindowSettings.Default
    do nativeSettings.Size <- Vector2i(width, height)
    do nativeSettings.Title <- "Orbiting Outer Space"
    
    let mutable keys = HashSet<Key>()
    let mutable keyActions = Map.empty
    let mutable gamePosition = Position.create 0.<sq> 0.<sq>
    
    let frameTimer = new Stopwatch()
    do frameTimer.Start()
    let scheduler = new Scheduler()
    
    do scheduler.AddSchedule (0.25<second>) (fun () ->
        gamePosition |> Location |> sender
        true)
    
    let gw = new GameWindow(windowSettings, nativeSettings)
    
    let onUpdate (args: FrameEventArgs) =
        gw.ProcessEvents() |> ignore
        let elapsed = frameTimer.ElapsedTicks
        frameTimer.Restart()
        let timeSince = ((elapsed |> float) / (Stopwatch.Frequency |> float)) * (1.<second>)
        
        scheduler.Cycle timeSince
        
        let newPosition = Player.processMovement (keys |> Seq.toList) gamePosition timeSince
        if newPosition <> gamePosition then
            gamePosition <- newPosition
        
        ()
        
    let onRender (args: FrameEventArgs) =
        gw.MakeCurrent()
        GL.Clear(ClearBufferMask.ColorBufferBit)
        
//        let vBufferObject = GL.GenBuffer()
//        let vertices =
//            [
//                -0.5; 0.5; 0.
//                -0.5; -0.5; 0.
//                0.5; -0.5; 0.
//                0.5; 0.5; 0.
//            ]
//            |> List.toArray
//        GL.BindBuffer(BufferTarget.ArrayBuffer, vBufferObject)
//        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof<float>, vertices, BufferUsageHint.StaticDraw)
//        vertices |> ignore
        
        gw.SwapBuffers()
        
    let onResize (args: ResizeEventArgs) =
        gw.Size <- Vector2i(width, height)
//        GL.Viewport(0, 0, width, height)
        
    let onKeyDown (ParseGameEvent (key, repeat)) =
        if repeat then
            keys.Add key |> ignore
        else
            keyActions
            |> Map.tryFind key
            |> function
                | Some a -> a()
                | None -> ()
        
    let onKeyUp (ParseGameEvent (key, repeat)) =
        keys.Remove(key) |> ignore
        ()
        
    let registerKeyAction (action: unit -> unit) (key: Key) =
        keyActions <- keyActions |> Map.add key action
        
    let onLoad () =
//        GL.ClearColor(Color.Black)
//        let textures = Assets.loadTextures ()
        
        ()
        
    do gw.add_Load(Action(onLoad))
    do gw.add_UpdateFrame(Action<FrameEventArgs>(onUpdate))
    do gw.add_RenderFrame(Action<FrameEventArgs>(onRender))
    do gw.add_Resize(Action<ResizeEventArgs>(onResize))
    do gw.add_KeyDown(Action<KeyboardKeyEventArgs>(onKeyDown))
    do gw.add_KeyUp(Action<KeyboardKeyEventArgs>(onKeyUp))
    
    member this.AddSchedule (interval: float<second>) (cb: unit -> bool) =
        scheduler.AddSchedule interval cb
    
    member this.Run() =
        gw.Run()
        
    interface IDisposable with
        member this.Dispose () =
            gw.Dispose()

