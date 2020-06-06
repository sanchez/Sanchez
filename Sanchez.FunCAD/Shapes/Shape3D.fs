namespace Sanchez.FunCAD.Shapes

open Sanchez.FunCAD
open Sanchez.Data.Positional
open Sanchez.ThreeWin

type Shape3DFacePoint = Vector<decimal<mm>>

type Shape3D =
    {
        Faces: (Shape3DFacePoint*Shape3DFacePoint*Shape3DFacePoint) array
    }
    
module Shape3D =
    let create faces =
        {
            Shape3D.Faces = faces
        }
        
    let rasterize shaders colorizer (s: Shape3D) =
        let points =
            s.Faces
            |> Array.map (fun (a, b, c) -> [| a; b; c |])
            |> Array.fold Array.append [||]
            |> Array.distinct
        let convertedPoints =
            points
            |> Seq.map (Vector.map (decimal >> float32))
            |> Seq.toArray
        let findPointPos p = Array.findIndex ((=) p) points
            
        let indices =
            s.Faces
            |> Seq.map (fun (a, b, c) ->
                (a |> findPointPos, b |> findPointPos, c |> findPointPos))
            |> Seq.toArray
        
        Vertexor.createColoredObject shaders ShaderSimple colorizer convertedPoints indices