module Sanchez.Serializer.Serial

open Sanchez.Serializer.Core

let hello = "hello"

let serializeToGeneric = Serializer.serializeToGeneric
let deserializeFromGeneric<'T> a = Serializer.deserializeFromGeneric<'T> a