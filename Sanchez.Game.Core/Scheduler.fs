namespace Sanchez.Game.Core

open FSharp.Data.UnitSystems.SI.UnitNames

type ScheduledAction = unit -> bool
type Schedule =
    {
        Action: ScheduledAction
        Interval: float<second>
        Remainder: float<second>
    }

type Scheduler() =
    let mutable schedules = []
    
    member this.AddSchedule interval action =
        if action() then
            let s = { Schedule.Action = action; Interval = interval; Remainder = 0.<second> }
            schedules <- s::schedules
            
    member this.Cycle interval =
        schedules <-
            schedules
            |> List.choose (fun x ->
                if x.Remainder <= 0.<second> then
                    if x.Action() then
                        { x with Remainder = (x.Interval - interval) } |> Some
                    else None
                else
                    { x with Remainder = (x.Remainder - interval) } |> Some)

