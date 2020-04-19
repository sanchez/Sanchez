namespace Sanchez.OOS.Core

open Sanchez.Game.Core

type User (conn: string, userName: string) =
    let mutable position = Position.create (0.<sq>) (0.<sq>)
    
    member this.Connection = conn
    member this.UserName = userName
    member this.Position = position
    
    member this.UpdatePosition (pos) =
        position <- pos
        
        
        
        
type UserManager() =
    let mutable users = []
    
    member this.RegisterUser (u: User) =
        users <- u::users
        
    member this.AllUsers () = users
        
    member this.FindUser (conn: string) =
        users
        |> List.tryFind (fun x -> x.Connection = conn)        