module Sanchez.OOS.Client.Player

open Sanchez.OOS.Client.Keys
open Sanchez.OOS.Core.GameCore
open FSharp.Data.UnitSystems.SI.UnitNames

let processMovement (keys: Key list) (currentPosition: Position) (frameDuration: float<second>) =
    let isDown k = keys |> List.tryFind ((=) k) |> Option.isSome
    
    let speed = 1.<sq/second>
    let distance = speed * frameDuration
    
    let leftVector =
        if isDown Key.Left then
            Position.create -1.<sq> 0.<sq>
        else Position.create 0.<sq> 0.<sq>
        
    let rightVector =
        if isDown Key.Right then
            Position.create 1.<sq> 0.<sq>
        else Position.create 0.<sq> 0.<sq>
        
    let posDirection = leftVector + rightVector
    let finalDirection = posDirection * distance
    
    currentPosition + finalDirection