module Sanchez.Serializer.Tests.ConverterTest

open NUnit.Framework
open FsUnitTyped
open Sanchez.Serializer

type SimpleTest =
    {
        SomeNumber: int
        SomeFloat: double
        SomeString: string
    }
    
type SimpleUnion =
    | TestBool of bool
    | TestString of string
    | TestRecord of SimpleTest
    | TestTuple of string*string
    | TestEmpty

[<Test>]
let genericConversion () =
    let test = { SimpleTest.SomeNumber = 12; SomeFloat = 2.0; SomeString = "Hello World" }
    let res = Serial.serializeToGeneric test
    
    Assert.Pass()
    
[<Test>]
let unionConversion () =
    let resEmpty = Serial.serializeToGeneric TestEmpty
    
    let testTuple = ("hello", "world") |> TestTuple
    let resTuple = Serial.serializeToGeneric testTuple
    
    let testRecord = { SimpleTest.SomeFloat = 0.2; SomeNumber = 3; SomeString = "testing" } |> TestRecord
    let resRecord = Serial.serializeToGeneric testRecord
    
    let test = "hello world" |> TestString
    let res = Serial.serializeToGeneric test
    
    Assert.Pass()