module Sanchez.Serializer.Tests.ConverterTest

open NUnit.Framework
open FsUnitTyped
open Sanchez.Serializer
open Sanchez.Serializer.Core.Types

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

let shouldResultEqual expected actual =
    match actual with
    | Ok s -> s |> shouldEqual expected
    | Error _ -> Assert.Fail "Encountered Result Error"
    
let evalResult actual =
    match actual with
    | Ok s -> s
    | Error _ -> failwith "Encountered Result Error"

[<Test>]
let genericConversion () =
    let test = { SimpleTest.SomeNumber = 12; SomeFloat = 2.0; SomeString = "Hello World" }
    let res = Serial.serializeToGeneric test
    
    let completeMap =
        Map.empty
        |> Map.add "SomeNumber" (12 |> double |> NumberSymbol)
        |> Map.add "SomeFloat" (2.0 |> NumberSymbol)
        |> Map.add "SomeString" ("Hello World" |> StringSymbol)
        |> ObjectSymbol
    
    res |> shouldResultEqual completeMap
    
[<Test>]
let fullPassConversion () =
    let test = { SimpleTest.SomeNumber = 12; SomeFloat = 2.0; SomeString = "Hello World" }
    let symbols = Serial.serializeToGeneric test
    let finalResult = symbols |> Result.bind Serial.deserializeFromGeneric<SimpleTest>
    
    finalResult |> shouldResultEqual test
    
[<Test>]
let testTuple () =
    let test = ("hello", "world")
    let resTuple = Serial.serializeToGeneric test
    
    let expected =
        [
            "hello" |> StringSymbol
            "world" |> StringSymbol
        ]
        |> ArraySymbol
        
    resTuple |> shouldResultEqual expected
    
    let returnResult = resTuple |> Result.bind Serial.deserializeFromGeneric<string*string>
    returnResult |> shouldResultEqual test
    
[<Test>]
let testTupleUnion () =
    let test = ("hello", "world") |> TestTuple
    let resTuple = Serial.serializeToGeneric test
    
    let expected =
        [
            "hello" |> StringSymbol
            "world" |> StringSymbol
        ]
        |> ArraySymbol
        |> (fun x -> ("TestTuple", x))
        |> UnionSymbol
        
    resTuple |> shouldResultEqual expected
    
    let returnResult = resTuple |> Result.bind Serial.deserializeFromGeneric<SimpleUnion>
    returnResult |> shouldResultEqual test
    
[<Test>]
let testEmptyUnion () =
    let resEmpty = Serial.serializeToGeneric TestEmpty
    
    resEmpty |> shouldResultEqual (("TestEmpty", EmptySymbol ()) |> UnionSymbol)
    
    let returnResult = resEmpty |> Result.bind Serial.deserializeFromGeneric<SimpleUnion>
    returnResult |> shouldResultEqual TestEmpty
    
[<Test>]
let testRecordUnion () =
    let test = { SimpleTest.SomeFloat = 0.2; SomeNumber = 3; SomeString = "testing" } |> TestRecord
    let resRecord = Serial.serializeToGeneric test
    
    let expected =
        Map.empty
        |> Map.add "SomeFloat" (0.2 |> NumberSymbol)
        |> Map.add "SomeNumber" (3 |> double |> NumberSymbol)
        |> Map.add "SomeString" ("testing" |> StringSymbol)
        |> ObjectSymbol
        |> (fun x -> UnionSymbol("TestRecord", x))
    
    resRecord |> shouldResultEqual expected
    
    let returnResult = resRecord |> Result.bind Serial.deserializeFromGeneric<SimpleUnion>
    returnResult |> shouldResultEqual test
    
let testStringUnion () =
    let test = "hello world" |> TestString
    let res = Serial.serializeToGeneric test
    
    let expected = ("TestString", "hello world" |> StringSymbol) |> UnionSymbol
    
    res |> shouldResultEqual expected
    
    let returnResult = res |> Result.bind Serial.deserializeFromGeneric<SimpleUnion>
    returnResult |> shouldResultEqual test