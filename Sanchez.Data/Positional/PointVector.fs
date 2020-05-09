namespace Sanchez.Data.Positional

open System

type PointVector<'T> =
    {
        X: 'T
        Y: 'T
    }
    
    static member inline (+) (a: PointVector<_>, b: PointVector<_>) =
        { PointVector.X = a.X + b.X; Y = a.Y + b.Y }
    static member inline (-) (a: PointVector<_>, b: PointVector<_>) =
        { PointVector.X = a.X - b.X; Y = a.Y - b.Y }

module PointVector =
    let create x y =
        { PointVector.X = x; Y = y }
    
    let inline mag (a: PointVector<_>) =
        Math.Sqrt((a.X * a.X) + (a.Y * a.Y))
        
    let inline normalize (a: PointVector<_>) =
        let dist = mag a
        { PointVector.X = a.X / dist; PointVector.Y = a.Y / dist }
        
    let map f (a: PointVector<_>) =
        { PointVector.X = f a.X; Y = f a.Y }