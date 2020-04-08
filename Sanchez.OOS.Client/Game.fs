﻿namespace Sanchez.OOS.Client

open OpenToolkit.Mathematics
open OpenToolkit.Windowing.Common
open System
open System.Collections.Generic
open System.Diagnostics
open OpenToolkit.Windowing.Desktop
open Sanchez.OOS.Client.Keys
open Sanchez.OOS.Core.GameCore
open FSharp.Data.UnitSystems.SI.UnitNames
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
    
    member this.AddSchedule (interval: float<second>) (cb: unit -> bool) =
        scheduler.AddSchedule interval cb
    
    member this.Run() =
        gw.Run()
        
    interface IDisposable with
        member this.Dispose () =
            gw.Dispose()

