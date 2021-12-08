module BoleroSample.Client.HomePage

open Bolero
open Elmish
open Bolero.Html
open Component

type Model = NoModel

type Message = Empty

type MakeComponent<'globalModel, 'globalMessage>
  (
    mdw: ModelWrapper<Model, 'globalModel>,
    msw: MessageWrapper<Message, 'globalMessage>,
    counterHRef: Attr,
    dataHRef: Attr
  ) =
  inherit Component<Model, Message, 'globalModel, 'globalMessage>(mdw, msw)
  override this.InitialModel = Model.Init()
  override this.OnUpdate (services: Services) (message: Message) (model: Model) : Model * Cmd<Message> = model, Cmd.none

  override this.View
    (model: Model)
    (dispatch: Message -> unit)
    (showSubView: SubViewDisplay<'globalModel, 'globalMessage>)
    =
    div [ attr.``class`` "content" ] [
      h1 [ attr.``class`` "title" ] [
        text "Welcome to Bolero!"
      ]
      p [] [
        text "This application demonstrates Bolero's major features."
      ]
      ul [] [
        li [] [
          text "The entire application is driven by "
          a [ attr.target "_blank"
              attr.href "https://fsbolero.github.io/docs/Elmish" ] [
            text "Elmish"
          ]
          text "."
        ]
        li [] [
          text "The menu on the left switches pages based on "
          a [ attr.target "_blank"
              attr.href "https://fsbolero.github.io/docs/Routing" ] [
            text "routes"
          ]
          text "."
        ]
        li [] [
          text "The "
          a [ counterHRef ] [ text "Counter" ]
          text " page demonstrates event handlers and data binding in "
          a [ attr.target "_blank"
              attr.href "https://fsbolero.github.io/docs/Templating" ] [
            text "HTML templates"
          ]
          text "."
        ]
        li [] [
          text "The "
          a [ dataHRef ] [ text "Download data" ]
          text " page demonstrates the use of HTTP requests to the server."
        ]
        p [] [
          text "Enjoy writing awesome apps!"
        ]
      ]
    ]
