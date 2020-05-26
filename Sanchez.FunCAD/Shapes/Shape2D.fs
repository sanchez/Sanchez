namespace Sanchez.FunCAD.Shapes

open Sanchez.FunCAD
open Sanchez.Data.Positional
open Sanchez.ThreeWin

type Shape2D =
    {
        Points: PointVector<decimal<mm>> list
    }
    
module Shape2D =
    let create points =
        {
            Shape2D.Points = points
        }
        
    let rasterize shaders colorizer (s: Shape2D) =
        s.Points
        |> List.map (fun x ->
            Vector.create x.X 0m<mm> x.Y
            |> Vector.map (decimal >> float32))
        |> Vertexor.createColoredLine shaders colorizer
        
    let rasterizePoints shaders colorizer sizer (s: Shape2D) =
        s.Points
        |> List.map (fun x ->
            Vector.create x.X 0m<mm> x.Y
            |> Vector.map (decimal >> float32))
        |> Vertexor.createColoredVertices shaders colorizer sizer
        
    let plotAlongPoints (s: Shape2D) (points: Vector<decimal<mm>> array) =
        let lastIndex = points.Length - 1
        let convertAndNormalize (a: Vector<decimal<mm>>) =
            a
            |> Vector.map (decimal >> float)
            |> Vector.normalize
        let conv (a: Vector<decimal<mm>>) = a |> Vector.map (decimal >> float)
        
        let shapePoints =
            s.Points
            |> Seq.map (PointVector.map (decimal >> float))
            |> Seq.toArray
        
        points
        |> Array.mapi (fun i x ->
            let pl =
                match i with
                | 0 ->
                    let next = points.[i + 1]
                    let posV = next - x
                    
                    { Plane.BasePoint = x |> conv; Normal = posV |> convertAndNormalize }
                | j when j = lastIndex ->
                    let prev = points.[i - 1]
                    let posV = x - prev
                    
                    { Plane.BasePoint = x |> conv; Normal = posV |> convertAndNormalize }
                | _ ->
                    let prev = points.[i - 1]
                    let next = points.[i + 1]
                    
                    let dir =
                        prev - next
                        |> convertAndNormalize
                    
                    { Plane.BasePoint = x |> conv; Normal = dir }
            shapePoints
            |> Array.map (fun x -> Plane.projectToPlane x pl)
            |> Array.map (Vector.map (decimal >> ((*) 1m<mm>))))
        
    let extrudeAlongPoints (s: Shape2D) (points: Vector<decimal<mm>> array) =
        plotAlongPoints s points
        |> Array.pairwise
        |> Array.map (fun (first, second) ->
            let firstPair = first |> Array.pairwise
            let secondPair = second |> Array.pairwise
            Seq.zip firstPair secondPair
            |> Seq.map (fun ((firstA, firstB), (secondA, secondB)) ->
                [| (firstA, firstB, secondB); (secondB, secondA, firstA) |])
            |> Seq.fold Array.append [||]
            )
        |> Array.fold Array.append [||]