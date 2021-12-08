module BoleroSample.Client.Counter

open Elmish
open Bolero.Html
open Component

type Model = { counter: int }

type Message =
  | Increment
  | Decrement
  | SetCounter of int

type MakeComponent<'globalModel, 'globalMessage>
  (
    mdw: ModelWrapper<Model, 'globalModel>,
    msw: MessageWrapper<Message, 'globalMessage>
  ) =
  inherit Component<Model, Message, 'globalModel, 'globalMessage>(mdw, msw)
  override this.InitialModel = { counter = 0 }

  override this.OnUpdate (services: Services) (message: Message) (model: Model) : Model * Cmd<Message> =
    match message with
    | Increment ->
      { model with
          counter = model.counter + 1 },
      Cmd.none
    | Decrement ->
      { model with
          counter = model.counter - 1 },
      Cmd.none
    | SetCounter value -> { model with counter = value }, Cmd.none

  override this.View
    (model: Model)
    (dispatch: Message -> unit)
    (showSubView: SubViewDisplay<'globalModel, 'globalMessage>)
    =
    concat [ h1 [ attr.``class`` "title" ] [
               text "A simple counter"
             ]
             p [] [
               button [ on.click (fun _ -> dispatch Decrement)
                        attr.``class`` "button" ] [
                 text "-"
               ]
               input [ attr.``type`` "number"
                       attr.id "counter"
                       attr.``class`` "input"
                       bind.input.int model.counter (fun v -> dispatch (SetCounter v)) ]
               button [ on.click (fun _ -> dispatch Increment)
                        attr.``class`` "button" ] [
                 text "+"
               ]
             ] ]
