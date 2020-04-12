namespace Sanchez.Game.Platformer

open Sanchez.Game.Platformer.Entity
open Sanchez.Game.Platformer.Assets
open FSharp.Data.UnitSystems.SI.UnitNames
open Sanchez.Data
open Sanchez.Game.Core

type GameManager<'TTextureKey when 'TTextureKey : comparison>(title, width, height) =
    let loadingQueue = new Queue<unit -> unit>()
    let loadQueuedItems () =
        while loadingQueue.Peek() |> Option.isSome do
            match loadingQueue.Pop() with
            | Some cb -> cb()
            | None -> ()
            
    let texManager = new TextureManager<'TTextureKey>()
    let goManager = new GameObjectManager()
    
    let onLoad () =
        loadQueuedItems()
        ()
        
    let onUpdate (timeSince: float<second>) =
        loadQueuedItems()
        goManager.Update(timeSince)
        
    let onRender () =
        goManager.Render()
    
    let game = new Game(title, width, height, onLoad, onUpdate, onRender)
    
    member this.LoadTexture (key: 'TTextureKey, fileName: string, ?animationDeets: int*float<FPS>) =
        match animationDeets with
        | Some x -> loadingQueue.Queue(fun () -> texManager.LoadTexture(key, fileName, x) |> ignore)
        | None -> loadingQueue.Queue(fun () -> texManager.LoadTexture(key, fileName) |> ignore)
        
    member this.LoadGameObject () =
        goManager.LoadGameObject()
        
    member this.Launch () =
        ()
        
    member this.Run () =
        game.Run()