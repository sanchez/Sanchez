module Sanchez.OOS.Client.Window.Characters.NetworkManager

open Sanchez.Data
open Sanchez.Game.Core
open Sanchez.Game.Platformer
open Sanchez.Game.Platformer.Entity
open Sanchez.OOS.Client.Window
open Sanchez.OOS.Client.Window.Assets
open Sanchez.OOS.Core
open FSharp.Data.UnitSystems.SI.UnitNames

type NetworkPlayer =
    {
        Name: string
        mutable CurrentPosition: Position
        mutable LastUpdate: Position
        mutable LastPosition: Position
        mutable LastPlayerDirection: PlayerDirection
        mutable IsMoving: bool
    }
    
let setupNetworkManager currentPlayerName (poster: ClientAction -> unit) (actioner: Actioner<string*ServerAction>) (manager: GameManager<Textures, Keys>) =
    let mutable playerList: NetworkPlayer list = []
    
    let networkHeadName = sprintf "network-%s-head"
    let networkBodyName = sprintf "network-%s-body"
    let networkNameName = sprintf "network-%s-name"
    
    let tweenPosition (p: NetworkPlayer) (timeElapsed: float<second>) =
        if p.IsMoving then
            let limitSpeed = (5.<sq/second>) * timeElapsed
            let midPointProjection = (p.LastUpdate - p.CurrentPosition) / 2.<sq>
            let finalProjection =
                if (midPointProjection |> Position.mag) > limitSpeed then
                    midPointProjection |> Position.unitify |> (fun x -> x * limitSpeed)
                else midPointProjection
            p.CurrentPosition <- p.CurrentPosition + (finalProjection)
        else
            p.CurrentPosition <- p.LastUpdate
    
    let removeObject (playerName: string) =
        playerName |> networkHeadName |> manager.RemoveObject
        playerName |> networkBodyName |> manager.RemoveObject
        playerName |> networkNameName |> manager.RemoveObject
        
    let addObject (pl: NetworkPlayer) =
        let headName = pl.Name |> networkHeadName
        let bodyName = pl.Name |> networkBodyName
        let nameName = pl.Name |> networkNameName
        
        let playerUpdate _ _ (timeElapsed: float<second>) =
            tweenPosition pl timeElapsed
            let tex =
                match (pl.IsMoving, pl.LastPlayerDirection) with
                | (true, PlayerLeft) -> TextureBodyLeft
                | (true, PlayerRight) -> TextureBodyRight
                | (false, PlayerLeft) -> TextureStationaryLeft
                | (false, PlayerRight) -> TextureStationaryRight
            (true, pl.CurrentPosition, tex)
            
        let headOffset = Position.create 0.<sq> 1.<sq>
        let headUpdate _ _ _ =
            let tex =
                match pl.LastPlayerDirection with
                | PlayerLeft -> TextureHeadLeft
                | PlayerRight -> TextureHeadRight
            (true, headOffset, tex)
            
        let playerName _ _ _ =
            (true, headOffset, pl.Name)
            
        manager.LoadTexturedGameObject bodyName playerUpdate
        manager.LoadTexturedGameObject headName <| TexturedGameObjectBinds.BindToObject<Textures> bodyName TextureBodyLeft headUpdate
        manager.LoadTextGameObject nameName <| TextGameObjectBinds.BindToObject<Textures> headName playerName
    
    actioner.AddActioner "networkManager" (fun (ip, a) ->
        match a with
        | ServerAction.PlayerUpdate players ->
            let newPlayerNames = players |> List.map (fun x -> x.Name) |> List.filter ((=) currentPlayerName >> not)
            let oldPlayerNames = playerList |> List.map (fun x -> x.Name)
            let removedPlayerNames = oldPlayerNames |> List.except newPlayerNames
            let newPlayerNames = newPlayerNames |> List.except oldPlayerNames
            
            removedPlayerNames |> List.iter removeObject
            let newObs =
                newPlayerNames
                |> List.map (fun x ->
                    {
                        NetworkPlayer.Name = x
                        LastUpdate = Position.create 0.<sq> 0.<sq>
                        LastPosition = Position.create 0.<sq> 0.<sq>
                        CurrentPosition = Position.create 0.<sq> 0.<sq>
                        LastPlayerDirection = PlayerRight
                        IsMoving = false
                    })
            newObs |> List.iter addObject
            
            let refinedPlayers =
                playerList
                |> List.filter (fun x -> removedPlayerNames |> List.contains (x.Name) |> not)
                |> (@) newObs
            refinedPlayers |> List.iter (fun x ->
                let newInfo = players |> List.find (fun y -> y.Name = x.Name)
                x.LastUpdate <- newInfo.Location
                x.LastPlayerDirection <- newInfo.Direction
                x.IsMoving <- newInfo.IsMoving)
            
            playerList <- refinedPlayers
            Some ()
        | SinglePlayerUpdate (name, player) ->
            if name = currentPlayerName then ()
            else
                match playerList |> List.tryFind (fun x -> x.Name = name) with
                | Some x ->
                    x.LastUpdate <- player.Location
                    x.IsMoving <- player.IsMoving
                    x.LastPlayerDirection <- player.Direction
                | None ->
                    let nPlayer =
                        {
                            NetworkPlayer.Name = player.Name
                            LastUpdate = player.Location
                            LastPosition = player.Location
                            CurrentPosition = player.Location
                            LastPlayerDirection = player.Direction
                            IsMoving = player.IsMoving
                        }
                    nPlayer |> addObject
                    playerList <- nPlayer::playerList
            Some()
        | _ -> None)