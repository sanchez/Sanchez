namespace Sanchez.FunCAD

open OpenToolkit.Mathematics
open Sanchez.ThreeWin

type Scene =
    {
        mutable SceneObjects: Vertexor list
    }
    
module Scene =
    let create () =
        { Scene.SceneObjects = [] }
        
    let addToScene a scene =
        scene.SceneObjects <- a::scene.SceneObjects
        scene
        
    let renderScene renderedCam scene =
        scene.SceneObjects
        |> List.iter (fun x ->
            Vertexor.renderVertexor x renderedCam Matrix4.Identity)