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
        
    let rasterize shaders colorizer sizer (s: Shape2D) =
        s.Points
        |> List.map (fun x ->
            Vector.create x.X 0m<mm> x.Y
            |> Vector.map (decimal >> float32))
        |> Vertexor.createColoredVertices shaders colorizer sizer