module BoleroSample.Client.RoutingMenu

open Elmish
open Bolero
open Bolero.Html
open Component

type Model<'page> = { page: 'page }

type Message<'page> = SetPage of 'page

type MenuEntry<'page, 'globalModel, 'globalMessage> =
  'page * string * Attr * (IComponent<'globalModel, 'globalMessage> list)

type MakeComponent<'globalModel, 'globalMessage, 'page when 'page: equality and 'page: comparison>
  (
    mdw: ModelWrapper<Model<'page>, 'globalModel>,
    msw: MessageWrapper<Message<'page>, 'globalMessage>,
    defaultPage: 'page,
    menuDefinition: MenuEntry<'page, 'globalModel, 'globalMessage> list
  ) =
  inherit Component<Model<'page>, Message<'page>, 'globalModel, 'globalMessage>(mdw, msw)
  override this.InitialModel = { page = defaultPage }

  override this.OnUpdate
    (services: Services)
    (message: Message<'page>)
    (model: Model<'page>)
    : Model<'page> * Cmd<Message<'page>> =
    match message with
    | SetPage page ->
      services.Log("SetPage")
      { model with page = page }, Cmd.none

  override this.View
    (model: Model<'page>)
    (dispatch: Message<'page> -> unit)
    (showSubView: SubViewDisplay<'globalModel, 'globalMessage>)
    =
    let menuItem (page: 'page, itemText: string, route: Attr, _) : Node =
      li [] [
        a [ attr.``class`` (
              if model.page = page then
                "is-active"
              else
                ""
            )
            route ] [
          text itemText
        ]
      ]

    let showPage (page: IComponent<'globalModel, 'globalMessage>) = showSubView page.Show

    let menuEntryToPageView ((id, _, _, content): MenuEntry<'page, 'globalModel, 'globalMessage>) : 'page * Node =
      let shows = List.map showPage content
      (id, Concat shows)

    let pageNodes: Map<'page, Node> =
      List.map menuEntryToPageView menuDefinition
      |> Map.ofList

    div [ attr.``class`` "columns" ] [
      aside [ attr.``class`` "column sidebar is-narrow" ] [
        section [ attr.``class`` "section" ] [
          nav [ attr.``class`` "menu" ] [
            ul [ attr.``class`` "menu-list" ] (List.map menuItem menuDefinition)
          ]
        ]
      ]
      div [ attr.``class`` "column" ] [
        section [ attr.``class`` "section" ] [
          cond model.page (fun page -> pageNodes.Item page)
        ]
      ]
    ]
