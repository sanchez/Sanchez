module Sanchez.OOS.Core.World.Generator

open Sanchez.Game.Core
open Sanchez.OOS.Core.Blocks

let generateStandardShip w =
    let createWallBlock pos =
        BaseBlock.createBasicBaseBlock (WallBlock.generateWallBlock() |> Wall) pos 100
    let spawnWallBlock x y =
        World.spawnBlock (createWallBlock <| Position.create x y)
        
    let world = World.createEmptyWorld()
        
    let floor =
        seq {
            for x in -10. .. 10. do
                yield Position.create (x * 1.<sq>) -1.<sq>
        }
        |> Seq.map createWallBlock
        |> Seq.fold (fun w b -> World.spawnBlock b w) world
        
    let leftWall =
        seq {
            for y in -1. .. 4. do
                yield Position.create -10.<sq> (y * 1.<sq>)
        }
        |> Seq.map createWallBlock
        |> Seq.fold (fun w b -> World.spawnBlock b w) floor
        
    leftWall