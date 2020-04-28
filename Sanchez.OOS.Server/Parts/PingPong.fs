module Sanchez.OOS.Server.Parts.PingPong

open Sanchez.Data
open Sanchez.OOS.Core
open Sanchez.Socketier.Server

let createPart (poster: SenderAddress*ServerAction -> unit) (actioner: Actioner<string*ClientAction>) (scheduler: Scheduler) =
    actioner.AddActioner "pingpong" (fun (ip, a) ->
        match a with
        | Ping id ->
            poster (SenderAddress.SingleSend ip, Pong id)
            Some ()
        | _ -> None)