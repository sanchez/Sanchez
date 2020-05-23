// Learn more about F# at http://fsharp.org

open System
open System.Drawing
open Sanchez.Data.Positional
open Sanchez.FunCAD
open Sanchez.FunCAD.Primitive

let generateGeometry shaderMap (scene: Scene) =
    
    let centerPoint =
        Point.create 0m<mm> 0m<mm> 0m<mm>
        |> Point.rasterize shaderMap Color.MediumVioletRed 4.f
    
    let centerBox =
        Box.create (Vector.create 0m<mm> 0m<mm> 0m<mm>) 1m<mm> 1m<mm> 1m<mm>
        |> Box.rasterize shaderMap Color.Firebrick
        
    scene
    |> Scene.addToScene centerPoint
    |> Scene.addToScene centerBox
    |> ignore

[<EntryPoint>]
let main _ =
    
    FunCAD.initialize "FunCAD Demo" generateGeometry
    
    0 // return an integer exit code
