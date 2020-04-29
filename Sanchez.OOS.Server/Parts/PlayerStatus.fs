module Sanchez.OOS.Server.Parts.PlayerStatus

open System
open Sanchez.Data
open Sanchez.OOS.Core
open Sanchez.Socketier.Server
open FSharp.Data.UnitSystems.SI.UnitNames

type PlayerStatusRecord = Player * DateTime

let createPart (poster: SenderAddress*ServerAction -> unit) (actioner: Actioner<string*ClientAction>) (scheduler: Scheduler) =
    let mutable players: PlayerStatusRecord list = []
    
    let updateForPlayer (pl: Player) =
        players <-
            players
            |> List.filter ((fun (p, _) -> p.Name = pl.Name) >> not)
            |> (fun x -> (pl, DateTime.UtcNow)::x)
            
    let removeOldPlayers () =
        let currentDT = DateTime.UtcNow
        players <-
            players
            |> List.filter (fun (_, dt) -> currentDT.Subtract(dt).TotalSeconds <= 5.)
        players |> List.map fst
    
    actioner.AddActioner "playerstatus" (fun (ip, a) ->
        match a with
        | PlayerUpdate pl ->
            updateForPlayer pl
            (SenderAddress.Broadcast, (pl.Name, pl) |> SinglePlayerUpdate) |> poster
            
            Some ()
        | _ -> None)
    
    scheduler.AddSchedule 0.2<second> (fun () ->
        (SenderAddress.Broadcast, removeOldPlayers() |> ServerAction.PlayerUpdate) |> poster
        true)