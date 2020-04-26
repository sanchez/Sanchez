module Sanchez.OOS.Server.ServerManager

open Sanchez.Data
open Sanchez.OOS.Core
open Sanchez.Socketier
open Sanchez.Socketier.Server

let createServer port cToken =
    asyncResult {
        let! (poster, actioner) = Server.createServer ClientAction.decode ServerAction.encode port cToken
        
        do! async {
            do! Async.Sleep 500
            return Ok ()
        }
        
        do actioner.AddActioner "pingpong" (fun (ip, a) ->
            match a with
            | Ping id ->
                poster (SenderAddress.SingleSend ip, Pong id)
                Some ())
        
        return ()
    }