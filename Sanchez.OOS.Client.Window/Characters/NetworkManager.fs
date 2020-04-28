module Sanchez.OOS.Client.Window.Characters.NetworkManager

open Sanchez.Data
open Sanchez.Game.Core
open Sanchez.Game.Platformer
open Sanchez.Game.Platformer.Entity
open Sanchez.OOS.Client.Window
open Sanchez.OOS.Client.Window.Assets
open Sanchez.OOS.Core

let setupNetworkManager currentPlayerName (poster: ClientAction -> unit) (actioner: Actioner<string*ServerAction>) (manager: GameManager<Textures, Keys>) =
    let mutable playerList: Player list = []
    
    let networkHeadName = sprintf "network-%s-head"
    let networkBodyName = sprintf "network-%s-body"
    let networkNameName = sprintf "network-%s-name"
    
    let removeObject (pl: Player) =
        pl.Name |> networkHeadName |> manager.RemoveObject
        pl.Name |> networkBodyName |> manager.RemoveObject
        pl.Name |> networkNameName |> manager.RemoveObject
        
    let addObject (pl: Player) =
        let headName = pl.Name |> networkHeadName
        let bodyName = pl.Name |> networkBodyName
        let nameName = pl.Name |> networkNameName
        
        let playerUpdate _ _ _ =
            (true, pl.Location, TextureStationaryLeft)
            
        let headOffset = Position.create 0.<sq> 1.<sq>
        let headUpdate _ _ _ =
            (true, headOffset, TextureHeadLeft)
            
        let playerName _ _ _ =
            (true, headOffset, pl.Name)
            
        manager.LoadTexturedGameObject bodyName playerUpdate
        manager.LoadTexturedGameObject headName <| TexturedGameObjectBinds.BindToObject<Textures> bodyName TextureBodyLeft headUpdate
        manager.LoadTextGameObject nameName <| TextGameObjectBinds.BindToObject<Textures> headName playerName
    
    actioner.AddActioner "networkManager" (fun (ip, a) ->
        match a with
        | ServerAction.PlayerUpdate players ->
            let refinedPlayers = players |> List.filter ((fun x -> x.Name = currentPlayerName) >> not)
            playerList |> List.iter removeObject
            refinedPlayers |> List.iter addObject
            
            playerList <- refinedPlayers
            Some ()
        | _ -> None)