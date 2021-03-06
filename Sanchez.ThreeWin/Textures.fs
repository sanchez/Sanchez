﻿namespace Sanchez.ThreeWin

open Sanchez.Data
open System.Drawing
open System.Drawing.Imaging
open System.IO
open FSharp.Data.UnitSystems.SI.UnitNames
open OpenToolkit.Graphics.OpenGL

type LoadedTexture = LoadedTexture of int

module Textures =
    let private loadBitmap (fileLocation: string) =
        try
            let i = new Bitmap(fileLocation)
            i.RotateFlip(RotateFlipType.RotateNoneFlipY)
            Some i
        with
        | :? FileNotFoundException as ex -> None
    
    let private createTexture (image: BitmapData) =
        let texId = GL.GenTexture()
        
        GL.BindTexture(TextureTarget.Texture2D, texId)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge)
        
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, image.Scan0)
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D)
        
        texId
        
    let private loadAnimatedImage (flipX: bool) (image: Bitmap) (frameWidth: int) =
        let frames = image.Width / frameWidth
        
        let imgs =
            Seq.init frames (fun i ->
                let left = i * frameWidth
                let croppedImage = image.Clone(System.Drawing.Rectangle(left, 0, frameWidth, image.Height), image.PixelFormat)
                if flipX then croppedImage.RotateFlip(RotateFlipType.RotateNoneFlipX)
                let data = croppedImage.LockBits(System.Drawing.Rectangle(0, 0, croppedImage.Width, croppedImage.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
                
                let id = createTexture data
                croppedImage.UnlockBits data
                id)
            |> Seq.map LoadedTexture
            |> Seq.toArray
        imgs
        
    let private loadStaticImage (flipX: bool) (image: Bitmap) =
        if flipX then image.RotateFlip(RotateFlipType.RotateNoneFlipX)
        let data = image.LockBits(System.Drawing.Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
        let id = createTexture data
        image.UnlockBits data
        
        id |> LoadedTexture
        
    let loadDoubleSidedStaticTexture fileName =
        opt {
            let! bitmap = loadBitmap fileName
            
            return (loadStaticImage false bitmap, loadStaticImage true bitmap)
        }
        
    let loadStaticTexture = loadBitmap >> (Option.map (loadStaticImage false))
    
    let loadDoubleSidedAnimatedTexture fileName frameWidth =
        opt {
            let! bitmap = loadBitmap fileName
            
            return (loadAnimatedImage false bitmap frameWidth, loadAnimatedImage true bitmap frameWidth)
        }
        
    let loadAnimatedTexture fileName frameWidth =
        loadBitmap fileName
        |> Option.map (fun x -> loadAnimatedImage false x frameWidth)