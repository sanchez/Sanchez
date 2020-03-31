module Sanchez.OOS.Server.Parts.Register

open Sanchez.OOS.Core

let handleServerRequest (serverPort: int) =
    {
        ServerStats.Port = serverPort
        Version = Action.staticVersion
    }
    |> Announcement