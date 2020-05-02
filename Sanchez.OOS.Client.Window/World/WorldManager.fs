module Sanchez.OOS.Client.Window.World.WorldManager

open OpenToolkit.Graphics.OpenGL
open OpenToolkit.Mathematics
open Sanchez.Data
open Sanchez.Game.Core
open Sanchez.Game.Platformer
open Sanchez.Game.Platformer.Assets
open Sanchez.Game.Platformer.Entity
open Sanchez.OOS.Client.Window
open Sanchez.OOS.Client.Window.Assets
open Sanchez.OOS.Core
open Sanchez.OOS.Core.World

type BlockRenderInformation = (int * Position)

type WorldManagerGameObject(worldFetcher: unit -> World, shader, sqToFloat: float<sq> -> float32) =
    let conversion = Matrix4.CreateScale (1.<sq> |> sqToFloat)
    
    let currentPosition = Position.create 0.<sq> 0.<sq>
    let mutable lastWorldData: BlockRenderInformation list = []
    
    let vertexArrayId = GameObjectBase.createTexturePoints shader
    
    interface IGameObject<Textures> with
        member this.CurrentPosition with get() = currentPosition
        member this.IsAlive with get() = true
        member this.Name with get() = "worldManager"
        
        member this.Update (obFinder, textureLoader, textLoader, timeElapsed) =
            let world = worldFetcher()
            lastWorldData <-
                world.Blocks
                |> List.choose (fun x ->
                    opt {
                        let! tex = TextureBlock1 |> textureLoader
                        let texId =
                            match tex with
                            | StaticTexture i -> i
                            | AnimatedTexture (is, fps) -> is |> Array.head
                        
                        return (texId, x.Position)
                    })
            ()
            
        member this.Render (widthScale) =
            Shader.useShader shader
            lastWorldData
            |> List.iter (fun x ->
                GL.BindTexture(TextureTarget.Texture2D, fst x)
                GL.BindVertexArray vertexArrayId
                
                let transformLoc = Shader.getUniformLocation shader "transform"
                let pos = x |> snd
                let trans = Matrix4.CreateTranslation(pos.X |> float32, pos.Y |> float32, 0.f)
                let scale = Matrix4.CreateScale(1.f / widthScale, 1.f, 1.f)
                let mat = trans * scale * conversion
                GL.UniformMatrix4(transformLoc, false, ref mat)
                
                GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0))
            

let setupWorldManager sqToFloat (poster: ClientAction -> unit) (actioner: Actioner<string*ServerAction>) (manager: GameManager<Textures, Keys>) =
    let mutable world = World.createEmptyWorld()
    
    let mutable worldGameObject = None
    manager.LoadCustomGameObject (fun textureFinder shader ->
        let ob = new WorldManagerGameObject((fun () -> world), shader, sqToFloat)
        worldGameObject <- Some ob
        ob)
    
    actioner.AddActioner "worldManager" (fun (ip, a) ->
        match a with
        | ServerAction.WorldUpdate w ->
            world <- w
            Some ()
        | _ -> None)