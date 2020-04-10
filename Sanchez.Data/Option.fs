namespace Sanchez.Data

[<AutoOpen>]
module Option =
    type OptionBuilder() =
        member this.Bind(v, f) = Option.bind f v
        member this.Return v = Some v
        member this.ReturnFrom o = o
        member this.Zero () = None
        
    let opt = OptionBuilder()

