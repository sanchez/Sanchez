module Sanchez.Serializer.Tests.Tests

open NUnit.Framework
open FsUnitTyped
open Sanchez.Serializer

[<SetUp>]
let Setup () =
    ()

[<Test>]
let Test1 () =
    1 |> shouldEqual 1
    
    Assert.Pass()
