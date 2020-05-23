namespace Sanchez.FunCAD

[<Measure>] type mm
[<Measure>] type cm
[<Measure>] type m

module Measurements =
    let cmToMM (a: decimal<cm>) = a * 10m<mm/cm>
    
    let mToMM (a: decimal<m>) = (a * 100m<cm/m>) |> cmToMM