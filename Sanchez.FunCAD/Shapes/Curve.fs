namespace Sanchez.FunCAD.Shapes

open Sanchez.Data.Positional
open Sanchez.FunCAD
open Sanchez.ThreeWin

type Curve<'T> =
    {
        DescribedBy: 'T -> Vector<decimal<mm>>
        Starting: 'T
        Ending: 'T
    }
    
module Curve =
    let create f s e =
        { Curve.DescribedBy = f; Starting = s; Ending = e }
        
    let offsetCurve v (c: Curve<_>) =
        {
            Curve.DescribedBy = (fun t -> (c.DescribedBy t) + v)
            Starting = c.Starting
            Ending = c.Ending
        }
        
    let inline resolve stepSize curve =
        let s = curve.Starting
        let e = curve.Ending
        seq { s .. stepSize .. e }
        |> Seq.map curve.DescribedBy
        
    let inline rasterize shaders colorizer stepSize (c: Curve<_>) =
        resolve stepSize c
        |> Seq.map (Vector.map (decimal >> float32))
        |> Seq.toList
        |> Vertexor.createColoredLine shaders colorizer
        
    let inline rasterizePoints shaders colorizer sizer stepSize (c: Curve<_>) =
        resolve stepSize c
        |> Seq.map (Vector.map (decimal>> float32))
        |> Seq.toList
        |> Vertexor.createColoredVertices shaders colorizer sizer