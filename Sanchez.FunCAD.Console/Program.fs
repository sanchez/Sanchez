// Learn more about F# at http://fsharp.org

open System
open System.Drawing
open Sanchez.Data.Positional
open Sanchez.FunCAD
open Sanchez.FunCAD.Primitive
open Sanchez.FunCAD.Shapes

let generateCurve shaderMap =
    let scene = Scene.create()
    let dist = 3.
    
    let colorizer _ = Color.Orange
    let sizer _ = 4.f
    let arcCreator (t: float<rads>) =
        let x = dist * (t |> float |> Math.Sin)
        let y = 0.
        let z = dist * (t |> float |> Math.Cos)
        Vector.create x y z
        |> Vector.map (decimal >> ((*) 1m<mm>))
    let arc =
        Curve.create arcCreator 0.<rads> (Math.PI * 1.<rads> / 2.)
    
    let simpleArc = Curve.rasterize shaderMap colorizer sizer 0.1<rads> arc
    let advancedArc = Curve.rasterize shaderMap (fun _ -> Color.SkyBlue) (fun _ -> 1.f) 0.01<rads> arc
    
    
    scene
    |> Scene.addToScene simpleArc
    |> Scene.addToScene advancedArc

let generateGeometry shaderMap (scene: Scene) =
    
    let centerPoint =
        Point.create 0m<mm> 0m<mm> 0m<mm>
        |> Point.rasterize shaderMap Color.MediumVioletRed 4.f
    
    let centerBox =
        Box.create (Vector.create 10m<mm> 0m<mm> 0m<mm>) 1m<mm> 1m<mm> 1m<mm>
        |> Box.rasterize shaderMap Color.Firebrick
        
    scene
    |> Scene.addToScene centerPoint
    |> Scene.addToScene centerBox
    |> Scene.addChildScene (generateCurve shaderMap)
    |> ignore

[<EntryPoint>]
let main _ =
    
    FunCAD.initialize "FunCAD Demo" generateGeometry
    
    0 // return an integer exit code
