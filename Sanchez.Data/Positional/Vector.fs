namespace Sanchez.Data.Positional

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
        
module Vector =
    let create x y z =
        { Vector.X = x; Y = y; Z = z }