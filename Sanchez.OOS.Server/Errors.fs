namespace Sanchez.OOS.Server

open Sanchez.Socketier.Errors

type ServerManagerErrors =
    | Connection of ConnectionErrors
    | FailedToStart