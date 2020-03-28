module Sanchez.Serializer.Core.Serializer

let serializeToGeneric = Converter.convertFromType
let deserializeFromGeneric<'T> a =
    Converter.convertToType (typeof<'T>) a
    |> function
        | Ok s -> s :?> 'T |> Ok
        | Error err -> Error err