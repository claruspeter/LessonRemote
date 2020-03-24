module Game.Hubs

open System.Collections.Generic
open System.Threading.Tasks
open Microsoft.AspNetCore.SignalR


type IGameHubCommands =
    abstract member CreateClass: string -> Task
    abstract member JoinClass: string -> Task
    abstract member NavigateTo: string -> string -> Task
    abstract member Refresh: unit -> Task

type IGameHub = 
    inherit IGameHubCommands
    abstract member Navigated: string -> string -> Task
    abstract member ClassJoined: string -> Task
    abstract member ReceivedMessage: string -> string -> Task

let classes = Dictionary<string, int>()

type GameHub() =
    inherit Hub<IGameHub>()
    
    interface IGameHubCommands with 
        member this.CreateClass name = this.CreateClass name
        member this.JoinClass name = this.JoinClass name
        member this.NavigateTo g s = this.NavigateTo g s
        member this.Refresh() = this.Refresh()

    member this.CreateClass (name: string) : Task =
        async{
            printfn "%s creating %A" this.UserName name
            match classes.ContainsKey name with
            | true ->
                return! this.Clients.Caller.ReceivedMessage name "Class already exists" |> Async.AwaitTask
                printfn "%s couldn't create %A" this.UserName name
            | false ->
                classes.Add(name, 0)
                return! this.Groups.AddToGroupAsync(this.Context.ConnectionId, name) |> Async.AwaitTask
                return! this.Clients.Caller.ClassJoined(name) |> Async.AwaitTask
                printfn "%s created %A" this.UserName name
        }
        |> Async.StartAsTask :> _

    member this.JoinClass (name: string) : Task =
        async{
            printfn "%s joining %A" this.UserName name
            match classes.ContainsKey name with
            | false ->
                return! this.Clients.Caller.ReceivedMessage name "Class not available" |> Async.AwaitTask
                printfn "%s couldn't join %A" this.UserName name
            | true ->
                return! this.Groups.AddToGroupAsync(this.Context.ConnectionId, name) |> Async.AwaitTask
                classes.[name] <- classes.[name] + 1
                return! this.Clients.Caller.ClassJoined(name) |> Async.AwaitTask
                return! this.Clients.OthersInGroup(name).ReceivedMessage name (sprintf "Number in group: %d" (classes.[name])) |> Async.AwaitTask
                printfn "%s joined %A" this.UserName name
        }
        |> Async.StartAsTask :> _

    member this.NavigateTo (groupName: string) (location: string) : Task =
        async{
            printfn "nav: %s -> %A" this.UserName location
            return! this.Clients.OthersInGroup(groupName).Navigated this.UserName location |> Async.AwaitTask
        }
        |> Async.StartAsTask :> _

    member this.Refresh () : Task =
        async{
            printfn "refreshing players"
            //return! this.Clients.All.AllPlayers (Actions.resolvedPlayersFor TheGame resolver None) |> Async.AwaitTask
        }
        |> Async.StartAsTask :> _

    member private this.UserName : string  =
        this.Context.ConnectionId

