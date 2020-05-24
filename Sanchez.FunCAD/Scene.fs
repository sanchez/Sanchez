namespace Sanchez.FunCAD

open OpenToolkit.Mathematics
open Sanchez.ThreeWin

type SceneObjects =
    | SceneObject of Vertexor
    | SceneGroup of Scene
and Scene =
    {
        mutable SceneObjects: SceneObjects list
    }
    
module Scene =
    let create () =
        { Scene.SceneObjects = [] }
        
    let addToScene a scene =
        scene.SceneObjects <- (a |> SceneObject)::scene.SceneObjects
        scene
        
    let addChildScene a scene =
        scene.SceneObjects <- (a |> SceneGroup)::scene.SceneObjects
        scene
        
    let rec renderScene renderedCam scene =
        scene.SceneObjects
        |> List.iter (function
            | SceneObject x -> Vertexor.renderVertexor x renderedCam Matrix4.Identity
            | SceneGroup scene -> renderScene renderedCam scene)