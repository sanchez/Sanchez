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
        
    let eyePosition (cam: OrbitalCamera) = cam.Position + cam.EyeOffset
        
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
        
    let private convertScreenSpaceToWorld (distFromEye: float32) (cam: OrbitalCamera) (rendered: RenderedCamera) (pos: PointVector<float32>) =
        let screenPosition = Vector4(cam.Position.X, cam.Position.Y, cam.Position.Z, 1.f) * rendered.View * rendered.Projection
        let mousePos = Vector4(pos.X * distFromEye, pos.Y * distFromEye, screenPosition.Z, distFromEye)
        let worldPos = mousePos * ((rendered.View * rendered.Projection) |> Matrix4.Invert)
        let normWorldPos = worldPos / worldPos.W
        Vector.create normWorldPos.X normWorldPos.Y normWorldPos.Z
        
    let prefillDistance f (cam: OrbitalCamera) =
        let dist = cam.EyeOffset |> Vector.map float |> Vector.mag |> float32
        f dist cam
        
    let mapMouseToView (distFromEye: float32) (cam: OrbitalCamera) (screenWidth: float) (screenHeight: float) (pos: PointVector<float32>) =
        let renderedCam = renderCamera cam ((screenWidth / screenHeight) |> float32)
        
        // I hate this but I it works and so I am walking away
        let screenPosition = Vector4(cam.Position.X, cam.Position.Y, cam.Position.Z, 1.f) * renderedCam.View * renderedCam.Projection
        
        let mousePos = Vector4(pos.X * distFromEye, pos.Y * distFromEye, screenPosition.Z, distFromEye)
        let worldPos = mousePos * ((renderedCam.View * renderedCam.Projection) |> Matrix4.Invert)
        let normWorldPos = worldPos / worldPos.W
        
        Vector.create normWorldPos.X normWorldPos.Y normWorldPos.Z
        
    let mapMouseToPosition = prefillDistance mapMouseToView
        
    let mapMouseToXZPlaneDist (distFromEye: float32) (cam: OrbitalCamera) (screenWidth: float) (screenHeight: float) (pos: PointVector<float32>) =
        let renderedCam = renderCamera cam ((screenWidth / screenHeight) |> float32)
        let worldSpace = convertScreenSpaceToWorld distFromEye cam renderedCam pos
        let dir = worldSpace - (eyePosition cam)
        Plane.create (Vector.create 0.f 0.f 0.f) (Vector.create 1.f 0.f 0.f) (Vector.create 0.f 0.f 1.f)
        |> Plane.pointOnPlane (cam |> eyePosition) dir
        
    let mapMouseToXZPlane = prefillDistance mapMouseToXZPlaneDist
        
    let mapMouseDeltaToViewMovementDist (distFromEye: float32) (cam: OrbitalCamera) (screenWidth: float) (screenHeight: float) (pos: PointVector<float32>) (delta: PointVector<float32>) =
        let renderedCam = renderCamera cam ((screenWidth / screenHeight) |> float32)
        
        let lastPointVector = pos - delta
        let lastViewVector = convertScreenSpaceToWorld distFromEye cam renderedCam lastPointVector
        let currentViewVector = convertScreenSpaceToWorld distFromEye cam renderedCam pos
        
        let difference = lastViewVector - currentViewVector
        
        Vector.create difference.X difference.Y 0.f
        
    let mapMouseDeltaToViewMovement (cam: OrbitalCamera) =
        let dist = cam.EyeOffset |> Vector.map float |> Vector.mag |> float32
        mapMouseDeltaToViewMovementDist dist