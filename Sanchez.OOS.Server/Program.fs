open System.Threading
open Sanchez.OOS.Core
open Sanchez.Socker

[<EntryPoint>]
let main argv =
    let port = 25599
    
    let cToken = new CancellationToken()
    let (actioner, poster) = Server.createServer ClientAction.decode ServerAction.encode port cToken
    actioner |> ignore
    
    actioner.AddActioner "test" (fun ip a -> None)
        
    while true do
        ()
    
    printfn "Hello World from F#!"
    0 // return an integer exit code
