module Sanchez.OOS.Client.Characters.NetworkPlayer

open Sanchez.Game.Core
open Sanchez.Game.Platformer
open Sanchez.OOS.Client
open Sanchez.OOS.Client.Assets

type NetworkPlayer (name, manager: GameManager<Textures, Keys>) =
    let mutable playerLastPosition = Position.create 0.<sq> 0.<sq>
    let mutable isAlive = true
    
    let generateKey k =
        sprintf "%s-%s" name k
    
    let playerUpdate obFinder position timeElapsed =
        (isAlive, playerLastPosition, TextureStationaryLeft)
        
    do manager.LoadTexturedGameObject (generateKey "player") playerUpdate
    
    member val Name = name with get
    member this.UpdatePosition (pos: Position) =
        playerLastPosition <- pos
        
    member this.Remove (manager: GameManager<Textures, Keys>) =
        isAlive <- false
    
    
type NetworkPlayerManager(manager: GameManager<Textures, Keys>) =
    let mutable players: NetworkPlayer list = []
    
    member this.HandlePlayerList (newPlayers: string list) =
        let currentPlayers = players |> List.map (fun x -> x.Name)
//        let removed =
//            newPlayers
//            |> List.except currentPlayers
//            |> List.map (fun x -> players |> List.find (fun y -> y.Name = x))
//        removed |> List.iter (fun x -> x.Remove(manager))
        let refinedList = players |> List.filter (fun x -> List.contains x.Name currentPlayers)
        
        let addedPlayers =
            newPlayers
            |> List.except currentPlayers
            |> List.map (fun x -> new NetworkPlayer(x, manager))
        
        players <- refinedList @ addedPlayers
        
    member this.HandlePlayerLocation (name: string) (position: Position) =
        let player =
            players
            |> List.tryFind (fun x -> x.Name = name)
            |> Option.defaultWith (fun () ->
                let p = new NetworkPlayer(name, manager)
                players <- p::players
                p)
            
        player.UpdatePosition position