namespace Sanchez.OOS.Client

open OpenToolkit.Mathematics
open OpenToolkit.Windowing.Common
open System
open System.Collections.Generic
open OpenToolkit.Windowing.Desktop
open Sanchez.OOS.Client.Keys

type Game (width, height) =
    let windowSettings = GameWindowSettings.Default
    do windowSettings.RenderFrequency <- 60.0
    do windowSettings.UpdateFrequency <- 60.0
    
    let nativeSettings = NativeWindowSettings.Default
    do nativeSettings.Size <- Vector2i(width, height)
    do nativeSettings.Title <- "Orbiting Outer Space"
    
    let mutable keys = HashSet<Key>()
    let mutable keyActions = Map.empty
    
    let gw = new GameWindow(windowSettings, nativeSettings)
    
    let onUpdate (args: FrameEventArgs) =
        gw.ProcessEvents() |> ignore
        
        ()
        
    let onRender (args: FrameEventArgs) =
        ()
        
    let onResize (args: ResizeEventArgs) =
        gw.Size <- Vector2i(width, height)
        
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
        ()
        
    do gw.add_Load(Action(onLoad))
    do gw.add_UpdateFrame(Action<FrameEventArgs>(onUpdate))
    do gw.add_RenderFrame(Action<FrameEventArgs>(onRender))
    do gw.add_Resize(Action<ResizeEventArgs>(onResize))
    do gw.add_KeyDown(Action<KeyboardKeyEventArgs>(onKeyDown))
    do gw.add_KeyUp(Action<KeyboardKeyEventArgs>(onKeyUp))
    
    member this.Run() =
        gw.Run()
        
    interface IDisposable with
        member this.Dispose () =
            gw.Dispose()

