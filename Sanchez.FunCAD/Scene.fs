namespace Sanchez.FunCAD

open System.Drawing
open OpenToolkit.Mathematics
open Sanchez.ThreeWin

type SceneObjects =
    | SceneObject of Vertexor
    | SceneGroup of Scene
and Scene =
    {
        mutable SceneObjects: SceneObjects list
        LightManager: LightManager
    }
    
module Scene =
    let create () =
        { Scene.SceneObjects = []; LightManager = LightManager.create() }
        
    let addToScene a scene =
        scene.SceneObjects <- (a |> SceneObject)::scene.SceneObjects
        scene
        
    let addChildScene a scene =
        scene.SceneObjects <- (a |> SceneGroup)::scene.SceneObjects
        scene
        
    let addLight l scene =
        scene.LightManager |> LightManager.addLight l |> ignore
        scene
        
    let addBasicLight pos scene =
        scene.LightManager |> LightManager.addPointLight pos (LightCurve.create 0.1f 0.1f 0.2f) (LightColor.createBasic Color.White) |> ignore
        scene
        
    let rec renderScene renderedCam scene =
        scene.SceneObjects
        |> List.iter (function
            | SceneObject x -> Vertexor.renderVertexor x scene.LightManager renderedCam Matrix4.Identity
            | SceneGroup scene -> renderScene renderedCam scene)