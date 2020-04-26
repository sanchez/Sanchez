module Sanchez.OOS.Server.ServerManager

open Sanchez.Data
open Sanchez.OOS.Core
open Sanchez.Socketier
open Sanchez.Socketier.Server
open FSharp.Data.UnitSystems.SI.UnitNames
open Sanchez.OOS.Server.Parts

let serverThread cToken poster actioner =
    Async.Start(async {
        do! Async.SwitchToNewThread()
        
        let scheduler = new Scheduler()
        
        do PingPong.createPart poster actioner
        
        do scheduler.Run 0.01<second>
    }, cToken)

let createServer port cToken =
    asyncResult {
        let! (poster, actioner) = Server.createServer ClientAction.decode ServerAction.encode port cToken
        
        do serverThread cToken poster actioner
        do! async {
            do! Async.Sleep 500
            return Ok ()
        }
    }