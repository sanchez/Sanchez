module Sanchez.OOS.Client.Window.Characters.MainPlayer

open Sanchez.Game.Platformer
open Sanchez.OOS.Client.Window
open Sanchez.OOS.Client.Window.Assets
open FSharp.Data.UnitSystems.SI.UnitNames
open Sanchez.Game.Core
open Sanchez.Game.Platformer.Entity
open Sanchez.OOS.Core

let loadMainPlayer (manager: GameManager<Textures, Keys>) name onPositionUpdate =
    let personSpeed = 5.<sq> / 1.<second>
    
    let mutable playerDir = PlayerRight
    let playerUpdate obFinder position timeElapsed =
        let sideWaysPosition =
            match (manager.IsKeyPressed LeftKey, manager.IsKeyPressed RightKey) with
            | (true, true) -> Position.create 0.<sq> 0.<sq>
            | (true, _) ->
                playerDir <- PlayerLeft
                Position.create -1.<sq> 0.<sq>
            | (_, true) ->
                playerDir <- PlayerRight
                Position.create 1.<sq> 0.<sq>
            | _ -> Position.create 0.<sq> 0.<sq>
        let distanceTravelled = personSpeed * timeElapsed
        let movementPos = ((sideWaysPosition) * distanceTravelled)
        
        let isStill = movementPos.X = 0.<sq>
        let tex =
            if isStill then
                match playerDir with
                | PlayerLeft -> TextureStationaryLeft
                | PlayerRight -> TextureStationaryRight
            elif movementPos.X < 0.<sq> then TextureBodyLeft
            else TextureBodyRight
            
        let finalPos = position + movementPos
        onPositionUpdate (finalPos, playerDir, not isStill)
        
        (true, finalPos, tex)
        
    let headOffset = Position.create 0.<sq> 1.<sq>
    let headUpdate obFinder position timeElapsed =
        let tex =
            match playerDir with
            | PlayerLeft -> TextureHeadLeft
            | PlayerRight -> TextureHeadRight
        (true, headOffset, tex)
        
    let playerName obFinder position timeElapsed =
        (true, headOffset, name)
        
    manager.LoadTexturedGameObject "mainPlayer" playerUpdate
    manager.LoadTexturedGameObject "mainPlayerHead" <| TexturedGameObjectBinds.BindToObject<Textures> "mainPlayer" TextureBodyRight headUpdate
    manager.LoadTextGameObject "mainPlayerText" <| TextGameObjectBinds.BindToObject<Textures> "mainPlayerHead" playerName