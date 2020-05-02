﻿namespace Sanchez.OOS.Client.Window.Assets

open Sanchez.Game.Platformer
open Sanchez.Game.Core
open FSharp.Data.UnitSystems.SI.UnitNames

type Textures =
    | TextureBodyLeft
    | TextureBodyRight
    | TextureStationaryLeft
    | TextureStationaryRight
    | TextureHeadLeft
    | TextureHeadRight
    
    | TextureBlock1
    
module Textures =
    let loadAllTextures (manager: GameManager<Textures, _>) =
        manager.LoadTexture(TextureBodyLeft, "Assets/body.png", true, (12, 24.<frame/second>))
        manager.LoadTexture(TextureBodyRight, "Assets/body.png", false, (12, 24.<frame/second>))
        manager.LoadTexture(TextureStationaryLeft, "Assets/stationaryBody.png", true)
        manager.LoadTexture(TextureStationaryRight, "Assets/stationaryBody.png", false)
        
        manager.LoadTexture(TextureHeadLeft, "Assets/heads/head1.png", true)
        manager.LoadTexture(TextureHeadRight, "Assets/heads/head1.png", false)
        
        manager.LoadTexture(TextureBlock1, "Assets/block1.png", false)