// Learn more about F# at http://fsharp.org

open System
open System.Drawing
open Sanchez.Data.Positional
open Sanchez.FunCAD
open Sanchez.FunCAD.Primitive
open Sanchez.FunCAD.Shapes

let generateLandForm shaderMap =
    let scene = Scene.create()
    
    let planeCreator (PointVectorTuple (x, y)) =
        Math.Sin((x |> decimal |> float) / 3.) * Math.Cos((y |> decimal |> float) / 3.)
        |> decimal
        |> (*) 2m<mm>
        
    let plane =
        Plane3D.create planeCreator (PointVector.create -10m<mm> -10m<mm>) (PointVector.create 10m<mm> 10m<mm>)
    
    scene
    |> Scene.addToScene (Plane3D.rasterize shaderMap (fun _ -> Color.Chartreuse) 1m<mm> plane)

let generatePipe shaderMap =
    let scene = Scene.create()
    
    let pipeRadius = 0.3<mm>
    let shape =
        Seq.append (seq { 0. .. 0.5 .. (Math.PI * 2.) }) (seq { yield (Math.PI * 2.) })
        |> Seq.map (fun t ->
            let x = pipeRadius * Math.Sin t
            let y = pipeRadius * Math.Cos t
            PointVector.create x y
            |> PointVector.map (decimal >> (*) 1m<mm>))
        |> Seq.toList
        |> Shape2D.create
        
    let pipeCreator (t: float<mm>) =
        let x = Math.Sin(t |> float)
        let y = 0.
        let z = t |> float
        Vector.create x y z
        |> Vector.map (decimal >> ((*) 1m<mm>))
        
    let pipe =
        Curve.create pipeCreator 0.<mm> 50.<mm>
        |> Curve.resolve 0.2<mm>
        |> Seq.toArray
        |> Shape2D.extrudeAlongPoints shape
        |> Shape3D.create
        
    scene
    |> Scene.addToScene (Shape2D.rasterize shaderMap (fun _ -> Color.Crimson) shape)
    |> Scene.addToScene (Shape3D.rasterize shaderMap (fun _ -> Color.Chocolate) pipe)

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
    
    let simpleArc = Curve.rasterize shaderMap colorizer 0.2<rads> arc
    let advancedArc =
        arc
        |> Curve.offsetCurve (Vector.create 0m<mm> 1m<mm> 0m<mm>)
        |> Curve.rasterize shaderMap (fun _ -> Color.SkyBlue) 0.01<rads>
    
    
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
//    |> Scene.addToScene centerPoint
    |> Scene.addToScene centerBox
    |> Scene.addChildScene (generateLandForm shaderMap)
//    |> Scene.addChildScene (generateCurve shaderMap)
//    |> Scene.addChildScene (generatePipe shaderMap)
    |> ignore

[<EntryPoint>]
let main _ =
    let cameraPos = Vector.create 100.f 100.f 100.f
    FunCAD.initialize "FunCAD Demo" cameraPos generateGeometry
    
    0 // return an integer exit code
