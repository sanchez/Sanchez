﻿namespace Sanchez.Data.Positional

open System

type Vector<'T> =
    {
        X: 'T
        Y: 'T
        Z: 'T
    }
    
    static member inline (+) (a: Vector<_>, b: Vector<_>) =
        {
            Vector.X = a.X + b.X
            Y = a.Y + b.Y
            Z = a.Z + b.Z
        }
    static member inline (-) (a: Vector<_>, b: Vector<_>) =
        {
            Vector.X = a.X - b.X
            Y = a.Y - b.Y
            Z = a.Z - b.Z
        }
    static member inline (*) (a: Vector<_>, b) =
        {
            Vector.X = a.X * b
            Y = a.Y * b
            Z = a.Z * b
        }
    static member inline (+*) (a: Vector<_>, b: Vector<_>) =
        {
            Vector.X = (a.Y * b.Z) - (a.Z * b.Y)
            Y = (a.Z * b.X) - (a.X * b.Z)
            Z = (a.X * b.Y) - (a.Y * b.X)
        }
    static member inline (.*) (a: Vector<_>, b: Vector<_>) =
        (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z)
        
module Vector =
    let create x y z =
        { Vector.X = x; Y = y; Z = z }
        
    let inline mag a =
        Math.Sqrt((a.X * a.X) + (a.Y * a.Y) + (a.Z * a.Z))
        
    let normalize a =
        let m = mag a
        {
            Vector.X = a.X / m
            Y = a.Y / m
            Z = a.Z / m
        }
        
    let map f a =
        {
            Vector.X = f a.X
            Y = f a.Y
            Z = f a.Z
        }