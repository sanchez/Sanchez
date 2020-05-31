namespace Sanchez.FunCAD.Shapes

open Sanchez.Data.Positional
open Sanchez.FunCAD
open Sanchez.ThreeWin

type Plane3D<'T> =
    {
        DescribedBy: PointVector<'T> -> 'T
        MinPoint: PointVector<'T>
        MaxPoint: PointVector<'T>
    }
    
module Plane3D =
    let create f s e =
        { Plane3D.DescribedBy = f; MinPoint = s; MaxPoint = e }
        
    let inline resolve stepSize plane =
        let s = plane.MinPoint
        let e = plane.MaxPoint
        seq {
            for x in s.X .. stepSize .. e.X do
                for y in s.Y .. stepSize .. e.Y ->
                    let v =
                        PointVector.create x y
                        |> plane.DescribedBy
                    Vector.create x v y
        }
        
    let inline progressiveResolve stepSize plane =
        let s = plane.MinPoint
        let e = plane.MaxPoint
        seq {
            for x in s.X..stepSize..e.X do
                yield
                    seq {
                        for y in s.Y..stepSize..e.Y do
                            let v =
                                PointVector.create x y
                                |> plane.DescribedBy
                            Vector.create x v y
                    }
        }
        
    let inline rasterize shaders colorizer stepSize (pl: Plane3D<_>) =
        let groupedPoints =
            progressiveResolve stepSize pl
            |> Seq.map (Seq.map (Vector.map (decimal >> float32)))
            |> Seq.pairwise
        let faces =
            seq {
                for (first, second) in groupedPoints do
                    let firstPair = first |> Seq.pairwise
                    let secondPair = second |> Seq.pairwise
                    for ((firstA, firstB), (secondA, secondB)) in (Seq.zip firstPair secondPair) do
                        yield! [ (firstA, firstB, secondB); (secondB, secondA, firstA) ]
            }
            |> Seq.toArray
            
        let points =
            query {
                for (a, b, c) in faces do
                    distinct
                    yield! [a; b; c]
            }
            |> Seq.toArray
        let convertedPoints =
            points
            |> Seq.map (Vector.map (decimal >> float32))
            |> Seq.toList
        let findPointPos p = Array.findIndex ((=) p) points
        
        let indices =
            faces
            |> Seq.map (fun (a, b, c) ->
                (a |> findPointPos, b |> findPointPos, c |> findPointPos))
            |> Seq.toList
            
        Vertexor.createColoredObject shaders ShaderSimple colorizer convertedPoints indices