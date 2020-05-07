namespace Sanchez.Data.Positional

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