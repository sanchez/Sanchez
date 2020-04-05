module Sanchez.OOS.Client.Connection.Users

open Sanchez.OOS.Core
open Sanchez.OOS.Core.GameCore

let registerUser (sender: ClientAction -> unit) (userName: string) =
    userName |> Register |> sender

let updatePosition (sender: ClientAction -> unit) (pos: Position) =
    pos |> Location |> sender