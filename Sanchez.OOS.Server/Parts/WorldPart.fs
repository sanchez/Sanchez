module Sanchez.OOS.Server.Parts.WorldPart

open System
open Sanchez.Data
open Sanchez.OOS.Core
open Sanchez.OOS.Core.World
open Sanchez.Socketier.Server
open FSharp.Data.UnitSystems.SI.UnitNames

let createPart (poster: SenderAddress*ServerAction -> unit) (actioner: Actioner<string*ClientAction>) (scheduler: Scheduler) =
    let mutable world =
        World.createEmptyWorld()
        |> Generator.generateStandardShip
    
    scheduler.AddSchedule 0.2<second> (fun () ->
        (SenderAddress.Broadcast, world |> ServerAction.WorldUpdate) |> poster
        true) 