namespace Sanchez.OOS.Core

open System.Net

module Action =
    let staticVersion = "0.0.1"
    
type ServerStats =
    {
        Port: int
        Version: string
    }
type FullConnection =
    {
        IP: IPAddress
        Port: int
        Version: string
    }

type ServerAction =
    | Announcement of ServerStats
    
type ClientAction =
    | ServerRequest of int
    
type ConnectionAction<'T> = IPAddress -> 'T -> unit option
type Actioner<'T>() =
    let mutable actioners = []
    
    member this.ExecuteAction (serverAddr: IPAddress, a: 'T) =
        actioners
        |> Seq.map (snd >> (fun x -> x serverAddr a))
        |> Seq.choose id
        |> Seq.tryHead
        |> function
            | Some () -> ()
            | None -> () // oh god, no actioners were able to execute
    
    member this.AddActioner (key: string, a: ConnectionAction<'T>) =
        actioners <- (key, a)::actioners
        
    member this.RemoveActioner (key: string) =
        actioners <- actioners |> List.filter (fst >> ((=) key) >> not)