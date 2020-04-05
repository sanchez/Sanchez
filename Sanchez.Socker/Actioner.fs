namespace Sanchez.Socker

open System.Net

type ConnectionAction<'T> = string -> 'T -> unit option
type Actioner<'T> () =
    let mutable actioners = []
    
    member this.ExecuteAction (serverAddr: string) (a: 'T) =
        actioners
        |> Seq.map (snd >> (fun x -> x serverAddr a))
        |> Seq.choose id
        |> Seq.tryHead
        |> function
            | Some () -> ()
            | None -> () // oh god, no actioners were able to execute
            
    member this.AddActioner (key: string) (a: ConnectionAction<'T>) =
        actioners <- (key, a)::actioners
        
    member this.RemoveActioner (key: string) =
        actioners <- actioners |> List.filter (fst >> ((=) key) >> not)