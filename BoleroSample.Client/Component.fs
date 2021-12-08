module BoleroSample.Client.Component

open Elmish
open Bolero
open System.Net.Http
open Microsoft.JSInterop

type Services =
  { httpClient : HttpClient
    jsRuntime : IJSRuntime }
  member this.Log (txt:string) =
    this.jsRuntime.InvokeVoidAsync("console.log", txt) |> ignore

type NoModel() =
  static member Init() = NoModel()

type MessageRouter<'globalMessage> = 'globalMessage -> 'globalMessage list

let messageRoutingFusion
  (routers: MessageRouter<'globalMessage> list)
  (message: 'globalMessage)
  (commands: Cmd<'globalMessage> list)
  : Cmd<'globalMessage> =
  (List.collect (fun mr -> mr message) routers)
  |> List.map Cmd.ofMsg
  |> List.append commands
  |> Cmd.batch

type Fusion<'globalMessage> = 'globalMessage -> Cmd<'globalMessage> list -> Cmd<'globalMessage>

type ModelWrapper<'local, 'glob>(wrap: 'glob -> 'local -> 'glob, unwrap: 'glob -> 'local) =
  member this.Wrap = wrap
  member this.UnWrap = unwrap

type MessageWrapper<'local, 'glob>
  (
    wrap: 'local -> 'glob,
    unwrap: 'glob -> 'local option,
    messageRouting: MessageRouter<'glob> list
  ) =
  member this.Wrap = wrap
  member this.UnWrap = unwrap
  member this.Fusion = messageRoutingFusion messageRouting

type IComponent<'globalModel, 'globalMessage> =
  abstract member Show : 'globalModel -> ('globalMessage -> unit) -> Node
  abstract member Update : Services -> 'globalMessage -> 'globalModel -> 'globalModel * Cmd<'globalMessage>
  abstract member Initialization : Cmd<'globalMessage>

let Initialize (components:IComponent<'globalModel, 'globalMessage> list) : Cmd<'globalMessage> =
  List.map (fun (c:IComponent<'globalModel, 'globalMessage>) -> c.Initialization) components |> Cmd.batch

type SubView<'globalModel, 'globalMessage> = 'globalModel -> ('globalMessage -> unit) -> Node
type SubViewDisplay<'globalModel, 'globalMessage> = SubView<'globalModel, 'globalMessage> -> Node

[<AbstractClass>]
type Component<'localModel, 'localMessage, 'globalModel, 'globalMessage>
  (
    mdw: ModelWrapper<'localModel, 'globalModel>,
    msw: MessageWrapper<'localMessage, 'globalMessage>
  ) =
  member this.µ : IComponent<'globalModel, 'globalMessage> = (this :> IComponent<'globalModel, 'globalMessage>)
  member this.Show = this.µ.Show
  member this.Update = this.µ.Update
  abstract member InitialModel : 'localModel
  abstract member View : 'localModel -> ('localMessage -> unit) -> SubViewDisplay<'globalModel, 'globalMessage> -> Node
  abstract member OnUpdate : Services -> 'localMessage -> 'localModel -> 'localModel * Cmd<'localMessage>

  abstract member InitialMessage : 'localMessage option
  default this.InitialMessage = None

  interface IComponent<'globalModel, 'globalMessage> with
    member this.Initialization : Cmd<'globalMessage> =
      match this.InitialMessage with
      | Some msg -> msw.Wrap msg |> Cmd.ofMsg
      | None -> Cmd.none
      
    member this.Show (model: 'globalModel) (dispatch: 'globalMessage -> unit) =
      let showSubView (subView: SubView<'globalModel, 'globalMessage>) : Node = subView model dispatch

      this.View(mdw.UnWrap model) (fun msg -> dispatch (msw.Wrap msg)) showSubView

    member this.Update
      (services: Services)
      (message: 'globalMessage)
      (model: 'globalModel)
      : 'globalModel * Cmd<'globalMessage> =
      let mdl, cmd =
        this.OnUpdate services (msw.UnWrap message).Value (mdw.UnWrap model)

      services.Log($"{mdl} {cmd}")

      (mdw.Wrap model mdl),
      [ Cmd.map (fun c -> msw.Wrap c) cmd ]
      |> msw.Fusion message
