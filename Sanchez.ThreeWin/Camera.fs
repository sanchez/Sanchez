namespace Sanchez.ThreeWin

open System
open OpenToolkit.Mathematics
open Sanchez.Data.Positional

type Camera =
    {
        mutable Position: Vector<float32>
        mutable LookingAt: Vector<float32>
        mutable Up: Vector<float32>
    }

type RenderedCamera =
    {
        Projection: Matrix4
        View: Matrix4
    }
    
module Camera =
    let create (pos) =
        {
            Camera.Position = pos
            LookingAt = Vector.create 0.f 0.f 0.f
            Up = Vector.create 0.f 1.f 0.f
        }
        
    let setPosition pos (cam: Camera) =
        cam.Position <- pos
        cam
    
    let setLookingAt pos (cam: Camera) =
        cam.LookingAt <- pos
        cam
        
    let renderCamera (cam: Camera) widthScale =
        let projection = Matrix4.CreatePerspectiveFieldOfView(Math.PI / 4. |> float32, widthScale, 0.1f, 100.f)
        let view = Matrix4.LookAt(
                          Vector3(cam.Position.X, cam.Position.Y, cam.Position.Z),
                          Vector3(cam.LookingAt.X, cam.LookingAt.Y, cam.LookingAt.Z),
                          Vector3(cam.Up.X, cam.Up.Y, cam.Up.Z)
                      )
        
        {
            RenderedCamera.Projection = projection
            View = view
        }