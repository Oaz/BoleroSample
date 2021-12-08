module BoleroSample.Client.Errors

open Elmish
open Bolero.Html
open Component

type Model = { error: string option }

type Message =
  | Error of exn
  | ClearError

let errorNotification errorText closeCallback =
  div [ attr.``class`` "notification is-warning" ] [
    cond closeCallback
    <| function
      | None -> empty
      | Some closeCallback ->
        button [ attr.``class`` "delete"
                 on.click closeCallback ] []
    text errorText
  ]

type MakeComponent<'globalModel, 'globalMessage>
  (
    mdw: ModelWrapper<Model, 'globalModel>,
    msw: MessageWrapper<Message, 'globalMessage>
  ) =
  inherit Component<Model, Message, 'globalModel, 'globalMessage>(mdw, msw)
  override this.InitialModel = { error = None }

  override this.OnUpdate (services: Services) (message: Message) (model: Model) : Model * Cmd<Message> =
    match message with
    | Error exn -> { model with error = Some exn.Message }, Cmd.none
    | ClearError -> { model with error = None }, Cmd.none

  override this.View
    (model: Model)
    (dispatch: Message -> unit)
    (showSubView: SubViewDisplay<'globalModel, 'globalMessage>)
    =
    div [ attr.id "notification-area" ] [
      cond model.error
      <| function
        | None -> empty
        | Some err -> errorNotification err (Some(fun _ -> dispatch ClearError))
    ]
