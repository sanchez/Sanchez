namespace Sanchez.ThreeWin

open OpenToolkit.Mathematics
open Sanchez.Data.Positional

type OrbitalCamera =
    {
        mutable Position: Vector<float32>
        mutable EyeOffset: Vector<float32>
        mutable Up: Vector<float32>
    }
    
module OrbitalCamera =
    let create eyeOffset =
        {
            OrbitalCamera.Position = Vector.create 0.f 0.f 0.f
            EyeOffset = eyeOffset
            Up = Vector.create 0.f 1.f 0.f
        }
        
    let setPosition pos (cam: OrbitalCamera) =
        cam.Position <- pos
        cam
    
    let setEyeOffset pos (cam: OrbitalCamera) =
        cam.EyeOffset <- pos
        cam
        
    let renderCamera (cam: OrbitalCamera) widthScale =
        let projection = Matrix4.CreatePerspectiveFieldOfView(System.Math.PI / 4. |> float32, widthScale, 0.1f, 100.f)
        let lookingAt = cam.Position
        let eyePosition = cam.Position + cam.EyeOffset
        let view = Matrix4.LookAt(
                                     Vector3(eyePosition.X, eyePosition.Y, eyePosition.Z),
                                     Vector3(lookingAt.X, lookingAt.Y, lookingAt.Z),
                                     Vector3(cam.Up.X, cam.Up.Y, cam.Up.Z)
                                 )
        
        {
            RenderedCamera.Projection = projection
            View = view
        }