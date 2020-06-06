namespace Sanchez.ThreeWin

open System.Drawing
open OpenToolkit.Graphics.ES30
open OpenToolkit.Graphics.OpenGL
open OpenToolkit.Mathematics
open Sanchez.Data.Positional

type Vertexor = Vertexor of (int * (LightManager -> RenderedCamera -> Matrix4 -> unit))

type ShaderTypes =
    | ShaderCustom of string
    | ShaderSimple
    | ShaderOutline

module Vertexor =
    let createEmpty () =
        (0, fun _ _ _ -> ()) |> Vertexor
    
    let renderVertexor (Vertexor (_, render)) =
        render
        
    let applyStaticTransformation (newTrans: Matrix4) (Vertexor (id, render)) =
        let newRender lm cam trans =
            trans * newTrans |> render lm cam
        (id, newRender) |> Vertexor
        
    let private generateArrayBuffer (vertices: float32[]) =
        GL.BindVertexArray 0
        let vertexBufferId = GL.GenBuffer()
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId)
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof<float32>, vertices, BufferUsageHint.StaticDraw)
        
        (vertexBufferId)
        
    let private generateArrayAndElementArrayBuffers (vertices: float32[]) (indices: uint32[]) =
        let vertexArrayId = GL.GenVertexArray()
        GL.BindVertexArray vertexArrayId
        
        let vertexBufferId = GL.GenBuffer()
        let elementBufferId = GL.GenBuffer()
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId)
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof<float32>, vertices, BufferUsageHint.StaticDraw)
        
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferId)
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof<uint32>, indices, BufferUsageHint.StaticDraw)
        
        (vertexArrayId, vertexBufferId, elementBufferId)
        
    let createStaticBackground (ShaderMap shaders) (colorLookup: PointVector<float32> -> Color) =
        let colorToFloats (c: Color) = ((c.R |> float32) / 255.f, (c.G |> float32) / 255.f, (c.B |> float32) / 255.f)
        let vertices =
            seq {
                yield PointVector.create 1.f 1.f
                yield PointVector.create -1.f 1.f
                yield PointVector.create -1.f -1.f
                yield PointVector.create 1.f -1.f
            }
            |> Seq.map (fun x ->
                let (r, g, b) = x |> colorLookup |> colorToFloats
                [| x.X; x.Y; r; g; b |])
            |> Seq.fold (Array.append) [||]
        let indices =
            [|
                0u; 1u; 2u
                2u; 3u; 0u
            |]
            
        let (vertexArrayId, _, _) = generateArrayAndElementArrayBuffers vertices indices
        
        let shader = shaders |> Map.find "staticBackgroundOverlay"
        let positionLocation = Shaders.getAttributeLocation shader "aPos"
        let colorLocation = Shaders.getAttributeLocation shader "aColor"
        
        GL.VertexAttribPointer(positionLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof<float32>, 0)
        GL.EnableVertexAttribArray(positionLocation)
        
        GL.VertexAttribPointer(colorLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof<float32>, 2 * sizeof<float32>)
        GL.EnableVertexAttribArray(colorLocation)
        
        let render lm (cam: RenderedCamera) (mat: Matrix4) =
            Shaders.useShader shader
            GL.BindVertexArray vertexArrayId
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0)
        (vertexArrayId, render) |> Vertexor
    
    let createColoredObject (ShaderMap shaders) (shaderType: ShaderTypes) (colorLookup: Vector<float32> -> Color) (vectors: Vector<float32> array) (indiceMap: (int * int * int) array) =
        let colorToFloats (c: Color) = ((c.R |> float32) / 255.f, (c.G |> float32) / 255.f, (c.B |> float32) / 255.f)
        let findAllFaces i =
            indiceMap
            |> Seq.filter (fun (a, b, c) -> a = i || b = i || c = i)
            |> Seq.map (fun (a, b, c) -> (vectors.[a], vectors.[b], vectors.[c]))
            |> Seq.map (fun (a, b, c) ->
                (b - a) +* (c - a))
            |> Seq.fold (+) (Vector.create 0.f 0.f 0.f)
        let vertices =
            vectors
            |> Seq.mapi (fun i x ->
                let normal = findAllFaces i
                let (r, g, b) = x |> colorLookup |> colorToFloats
                [| x.X; x.Y; x.Z; r; g; b; normal.X; normal.Y; normal.Z |])
            |> Seq.fold (Array.append) [||]
        let indices =
            indiceMap
            |> Seq.map (fun (a, b, c) ->
                [|
                    (a |> uint32)
                    (b |> uint32)
                    (c |> uint32)
                |])
            |> Seq.fold (Array.append) [||]
            
        let (vertexArrayId, _, _) = generateArrayAndElementArrayBuffers vertices indices
            
        let shader =
            (match shaderType with
            | ShaderCustom s -> s
            | ShaderSimple -> "simpleColor"
            |> Map.find) shaders
        let positionLocation = Shaders.getAttributeLocation shader "aPos"
        let colorLocation = Shaders.getAttributeLocation shader "aColor"
        let normalLocation = Shaders.getAttributeLocation shader "aNormal"
        let transformLocation = Shaders.getUniformLocation shader "transform"
        let viewLocation = Shaders.getUniformLocation shader "view"
        let projectionLocation = Shaders.getUniformLocation shader "projection"
        
        let viewPosLocation = Shaders.getUniformLocation shader "viewPos"
        let lightLocation i p =
            sprintf "pointLights[%d].%s" i p
            |> Shaders.getUniformLocation shader
        
        GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 9 * sizeof<float32>, 0)
        GL.EnableVertexAttribArray(positionLocation)
        
        GL.VertexAttribPointer(colorLocation, 3, VertexAttribPointerType.Float, false, 9 * sizeof<float32>, 3 * sizeof<float32>)
        GL.EnableVertexAttribArray(colorLocation)
        
        GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 9 * sizeof<float32>, 6 * sizeof<float32>)
        GL.EnableVertexAttribArray(normalLocation)
        
        let render (lm: LightManager) (cam: RenderedCamera) (mat: Matrix4) =
            Shaders.useShader shader
            GL.BindVertexArray vertexArrayId
            GL.Uniform3(viewPosLocation, Vector3(cam.Position.X, cam.Position.Y, cam.Position.Z))
            GL.UniformMatrix4(transformLocation, true, ref mat)
            GL.UniformMatrix4(viewLocation, true, ref cam.View)
            GL.UniformMatrix4(projectionLocation, true, ref cam.Projection)
            LightManager.renderPointLights 4 lightLocation lm
            
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0)
            
        (vertexArrayId, render) |> Vertexor
        
    let createTexturedObject (ShaderMap shaders) (shaderType: ShaderTypes) (vectors: (Vector<float32> * PointVector<float32>) list) (indiceMap: (int * int * int) list) (onUpdate: unit -> LoadedTexture) =
        let vertices =
            vectors
            |> Seq.map (fun (x, texCoord) ->
                [| x.X; x.Y; x.Z; texCoord.X; texCoord.Y |])
            |> Seq.fold (Array.append) [||]
        let indices =
            indiceMap
            |> Seq.map (fun (a, b, c) ->
                [|
                    (a |> uint32)
                    (b |> uint32)
                    (c |> uint32)
                |])
            |> Seq.fold (Array.append) [||]
            
        let (vertexArrayId, _, _) = generateArrayAndElementArrayBuffers vertices indices
            
        let shader =
            (match shaderType with
             | ShaderCustom s -> s
             | ShaderSimple -> "simpleTexture"
             |> Map.find) shaders
        let positionLocation = Shaders.getAttributeLocation shader "aPos"
        let textureLocation = Shaders.getAttributeLocation shader "aTexCoord"
        let transformLocation = Shaders.getUniformLocation shader "transform"
        let viewLocation = Shaders.getUniformLocation shader "view"
        let projectionLocation = Shaders.getUniformLocation shader "projection"
        
        GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof<float32>, 0)
        GL.EnableVertexAttribArray(positionLocation)
        
        GL.VertexAttribPointer(textureLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof<float32>, 3 * sizeof<float32>)
        GL.EnableVertexAttribArray(textureLocation)
        
        let render lm (cam: RenderedCamera) (mat: Matrix4) =
            Shaders.useShader shader
            GL.BindVertexArray vertexArrayId
            GL.UniformMatrix4(transformLocation, true, ref mat)
            GL.UniformMatrix4(viewLocation, true, ref cam.View)
            GL.UniformMatrix4(projectionLocation, true, ref cam.Projection)
            
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0)
            
        (vertexArrayId, render) |> Vertexor
        
    let createColoredVertices (ShaderMap shaders) (colorLookup: Vector<float32> -> Color) (pointSizeLookup: Vector<float32> -> float32) (vectors: Vector<float32> list) =
        let colorToFloats (c: Color) = ((c.R |> float32) / 255.f, (c.G |> float32) / 255.f, (c.B |> float32) / 255.f)
        let vertices =
            vectors
            |> Seq.map (fun x ->
                let (r, g, b) = x |> colorLookup |> colorToFloats
                let pSize = x |> pointSizeLookup
                [| x.X; x.Y; x.Z; r; g; b; pSize |])
            |> Seq.fold (Array.append) [||]
        
        let indices =
            Array.create (vectors.Length) (0 |> uint32)
            |> Array.mapi (fun i _ -> uint32 i)
        
        let (vertexArrayId, _, _) = generateArrayAndElementArrayBuffers vertices indices
        
        let shader = shaders |> Map.find "simplePoint"
        let positionLocation = Shaders.getAttributeLocation shader "aPos"
        let colorLocation = Shaders.getAttributeLocation shader "aColor"
        let pointSizeLocation = Shaders.getAttributeLocation shader "aPointSize"
        let transformLocation = Shaders.getUniformLocation shader "transform"
        let viewLocation = Shaders.getUniformLocation shader "view"
        let projectionLocation = Shaders.getUniformLocation shader "projection"
        
        GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 7 * sizeof<float32>, 0)
        GL.EnableVertexAttribArray(positionLocation)
        
        GL.VertexAttribPointer(colorLocation, 3, VertexAttribPointerType.Float, false, 7 * sizeof<float32>, 3 * sizeof<float32>)
        GL.EnableVertexAttribArray(colorLocation)
        
        GL.VertexAttribPointer(pointSizeLocation, 1, VertexAttribPointerType.Float, false, 7 * sizeof<float32>, 6 * sizeof<float32>)
        GL.EnableVertexAttribArray(pointSizeLocation)
        
        let render lm (cam: RenderedCamera) (mat: Matrix4) =
            Shaders.useShader shader
            GL.BindVertexArray vertexArrayId
            GL.UniformMatrix4(transformLocation, true, ref mat)
            GL.UniformMatrix4(viewLocation, true, ref cam.View)
            GL.UniformMatrix4(projectionLocation, true, ref cam.Projection)
            
            GL.DrawArrays(PrimitiveType.Points, 0, indices.Length)
//            GL.DrawElements(PrimitiveType.Points, vertices.Length, DrawElementsType.UnsignedInt, 0)
        
        (vertexArrayId, render) |> Vertexor
        
    let createColoredLine (ShaderMap shaders) (colorLookup: Vector<float32> -> Color) (vectors: Vector<float32> list) =
        let colorToFloats (c: Color) = ((c.R |> float32) / 255.f, (c.G |> float32) / 255.f, (c.B |> float32) / 255.f)
        let vertices =
            vectors
            |> Seq.map (fun x ->
                let (r, g, b) = x |> colorLookup |> colorToFloats
                let pSize = 1.f
                [| x.X; x.Y; x.Z; r; g; b; pSize |])
            |> Seq.fold (Array.append) [||]
        let indices =
            Array.create (vectors.Length) (0 |> uint32)
            |> Array.mapi (fun i _ -> uint32 i)
            
        let (vertexArrayId, _, _) = generateArrayAndElementArrayBuffers vertices indices
        
        let shader = shaders |> Map.find "simplePoint"
        let positionLocation = Shaders.getAttributeLocation shader "aPos"
        let colorLocation = Shaders.getAttributeLocation shader "aColor"
        let pointSizeLocation = Shaders.getAttributeLocation shader "aPointSize"
        let transformLocation = Shaders.getUniformLocation shader "transform"
        let viewLocation = Shaders.getUniformLocation shader "view"
        let projectionLocation = Shaders.getUniformLocation shader "projection"
        
        GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 7 * sizeof<float32>, 0)
        GL.EnableVertexAttribArray(positionLocation)
        
        GL.VertexAttribPointer(colorLocation, 3, VertexAttribPointerType.Float, false, 7 * sizeof<float32>, 3 * sizeof<float32>)
        GL.EnableVertexAttribArray(colorLocation)
        
        GL.VertexAttribPointer(pointSizeLocation, 1, VertexAttribPointerType.Float, false, 7 * sizeof<float32>, 6 * sizeof<float32>)
        GL.EnableVertexAttribArray(pointSizeLocation)
        
        let render lm (cam: RenderedCamera) (mat: Matrix4) =
            Shaders.useShader shader
            GL.BindVertexArray vertexArrayId
            GL.UniformMatrix4(transformLocation, true, ref mat)
            GL.UniformMatrix4(viewLocation, true, ref cam.View)
            GL.UniformMatrix4(projectionLocation, true, ref cam.Projection)
            
            GL.DrawArrays(PrimitiveType.LineStrip, 0, indices.Length)
            
        (vertexArrayId, render) |> Vertexor
            