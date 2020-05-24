namespace Sanchez.FunCAD

[<Measure>] type mm
[<Measure>] type cm
[<Measure>] type m

[<Measure>] type rads
[<Measure>] type degrees
[<Measure>] type t

module Measurements =
    let cmToMM (a: decimal<cm>) = a * 10m<mm/cm>
    
    let mToMM (a: decimal<m>) = (a * 100m<cm/m>) |> cmToMM
    
    let degToRads (a: float<degrees>) = (a * (System.Math.PI * 1.<rads>)) / 180.<degrees>