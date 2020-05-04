namespace Sanchez.ThreeWin

open System.Drawing
open OpenToolkit.Graphics.OpenGL
open OpenToolkit.Mathematics
open Sanchez.Data.Positional

type Vertexor = Vertexor of (int * (RenderedCamera -> Matrix4 -> unit))

module Vertexor =
    let createEmpty () =
        (0, fun _ _ -> ()) |> Vertexor
    
    let renderVertexor (Vertexor (_, render)) =
        render
        
    let applyStaticTransformation (newTrans: Matrix4) (Vertexor (id, render)) =
        let newRender cam trans =
            trans * newTrans |> render cam
        (id, newRender) |> Vertexor
    
    let createColoredObject (ShaderMap shaders) (colorLookup: Vector<float32> -> Color) (vectors: Vector<float32> list) (indiceMap: (int * int * int) list) =
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
            
        let vertexArrayId = GL.GenVertexArray()
        GL.BindVertexArray vertexArrayId
        
        let vertexBufferId = GL.GenBuffer()
        let elementBufferId = GL.GenBuffer()
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId)
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof<float32>, vertices, BufferUsageHint.StaticDraw)
        
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferId)
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof<uint32>, indices, BufferUsageHint.StaticDraw)
        
        let shader = shaders |> Map.find "simpleColor"
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