namespace Sanchez.ThreeWin

open System.Drawing
open OpenToolkit.Graphics.OpenGL
open OpenToolkit.Mathematics
open Sanchez.Data.Positional

type Vertexor = Vertexor of (int * (RenderedCamera -> Matrix4 -> unit))

type ShaderTypes =
    | ShaderCustom of string
    | ShaderSimple
    | ShaderOutline

module Vertexor =
    let createEmpty () =
        (0, fun _ _ -> ()) |> Vertexor
    
    let renderVertexor (Vertexor (_, render)) =
        render
        
    let applyStaticTransformation (newTrans: Matrix4) (Vertexor (id, render)) =
        let newRender cam trans =
            trans * newTrans |> render cam
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
        
        let render (cam: RenderedCamera) (mat: Matrix4) =
            Shaders.useShader shader
            GL.BindVertexArray vertexArrayId
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0)
        (vertexArrayId, render) |> Vertexor
    
    let createColoredObject (ShaderMap shaders) (shaderType: ShaderTypes) (colorLookup: Vector<float32> -> Color) (vectors: Vector<float32> list) (indiceMap: (int * int * int) list) =
        let colorToFloats (c: Color) = ((c.R |> float32) / 255.f, (c.G |> float32) / 255.f, (c.B |> float32) / 255.f)
        let vertices =
            vectors
            |> Seq.map (fun x ->
                let (r, g, b) = x |> colorLookup |> colorToFloats
                [| x.X; x.Y; x.Z; r; g; b |])
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
        let transformLocation = Shaders.getUniformLocation shader "transform"
        let viewLocation = Shaders.getUniformLocation shader "view"
        let projectionLocation = Shaders.getUniformLocation shader "projection"
        
        GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof<float32>, 0)
        GL.EnableVertexAttribArray(positionLocation)
        
        GL.VertexAttribPointer(colorLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof<float32>, 3 * sizeof<float32>)
        GL.EnableVertexAttribArray(colorLocation)
        
        let render (cam: RenderedCamera) (mat: Matrix4) =
            Shaders.useShader shader
            GL.BindVertexArray vertexArrayId
            GL.UniformMatrix4(transformLocation, true, ref mat)
            GL.UniformMatrix4(viewLocation, true, ref cam.View)
            GL.UniformMatrix4(projectionLocation, true, ref cam.Projection)
            
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
        
        let render (cam: RenderedCamera) (mat: Matrix4) =
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
        
        let render (cam: RenderedCamera) (mat: Matrix4) =
            Shaders.useShader shader
            GL.BindVertexArray vertexArrayId
            GL.UniformMatrix4(transformLocation, true, ref mat)
            GL.UniformMatrix4(viewLocation, true, ref cam.View)
            GL.UniformMatrix4(projectionLocation, true, ref cam.Projection)
            
            GL.DrawArrays(PrimitiveType.Points, 0, vertices.Length)
//            GL.DrawElements(PrimitiveType.Points, vertices.Length, DrawElementsType.UnsignedInt, 0)
        
        (vertexArrayId, render) |> Vertexor
            