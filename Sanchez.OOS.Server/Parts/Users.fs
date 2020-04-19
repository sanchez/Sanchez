module Sanchez.OOS.Server.Parts.Users

open Sanchez.OOS.Core
open Sanchez.Socker

let registerPart (actioner: Actioner<ClientAction>) (broadcaster: ServerAction -> unit) =
    let mutable userManager = new UserManager()
    
    actioner.AddActioner "userHandler" (fun ip ->
        function
            | Location pos ->
                let user = userManager.FindUser ip
                
                user |> Option.map (fun x -> x.UpdatePosition pos) |> ignore
                (ip, pos) |> LocationUpdate |> broadcaster
                
                Some ()
            | Register userName ->
                new User(ip, userName) |> userManager.RegisterUser
                
                userManager.AllUsers() |> Seq.map (fun x -> x.UserName) |> Seq.toArray |> Players |> broadcaster
                
                Some ()
            | _ -> None)
    
    ()