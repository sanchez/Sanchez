module Sanchez.OOS.Client.Characters.NetworkPlayer

open Sanchez.Game.Core

type NetworkPlayer (name) =
    let mutable playerLastPosition = Position.create 0.<sq> 0.<sq>
    
    member val Name = name with get