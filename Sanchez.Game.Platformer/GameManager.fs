namespace Sanchez.Game.Platformer

open Sanchez.Game.Platformer.Entity
open Sanchez.Game.Platformer.Assets
open FSharp.Data.UnitSystems.SI.UnitNames
open OpenToolkit.Graphics.OpenGL
open Sanchez.Data
open Sanchez.Game.Core

type GameManager<'TTextureKey, 'TKey when 'TTextureKey : comparison and 'TKey : comparison>(title, width, height, sqToFloat) =
    let mutable shader = None
    let loadingQueue = new Queue<unit -> unit>()
    let loadQueuedItems () =
        while loadingQueue.Peek() |> Option.isSome do
            match loadingQueue.Pop() with
            | Some cb -> cb()
            | None -> ()
            
    let texManager = new TextureManager<'TTextureKey>()
    let goManager = new GameObjectManager<'TTextureKey>(sqToFloat, texManager.FindTexture)
    
    let onLoad () =
        shader <- Shader.loadShaders() |> Some
        loadQueuedItems()
        ()
        
    let onUpdate (timeSince: float<second>) =
        loadQueuedItems()
        goManager.Update(timeSince)
        
    let onRender (widthScale: float32) =
        shader |> Option.map Shader.useShader |> ignore
        goManager.Render(widthScale)
        
    let game = new Game<'TKey>(title, width, height, onLoad, onUpdate, onRender)
    
    member this.LoadTexture (key: 'TTextureKey, fileName: string, flip: bool, ?animationDeets: int*float<frame/second>) =
        match animationDeets with
        | Some x -> loadingQueue.Queue(fun () -> texManager.LoadTexture(key, fileName, flip, x) |> ignore)
        | None -> loadingQueue.Queue(fun () -> texManager.LoadTexture(key, fileName, flip) |> ignore)
        
    member this.LoadGameObject onUpdate =
        loadingQueue.Queue(fun () ->
            Option.map (goManager.LoadGameObject onUpdate) shader |> ignore)
        
    member this.LoadCustomGameObject initializer =
        loadingQueue.Queue(fun () ->
            Option.map (initializer texManager.FindTexture) shader
            |> Option.map (fun x -> goManager.AddGameObject x)
            |> ignore)
        
    member this.IsKeyPressed = game.IsKeyPressed
    member this.RemoveKeyBinding = game.RemoveKeyBinding
    member this.WasKeyReleased = game.WasKeyReleased
    member this.AddKeyBinding = game.AddKeyBinding
        
    member this.Launch () =
        ()
        
    member this.Run () =
        game.Run()