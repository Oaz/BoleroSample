module BoleroSample.Client.Main

open System.Net.Http
open Microsoft.AspNetCore.Components
open Elmish
open Bolero
open Component

/// Routing endpoints definition.
type Page =
  | [<EndPoint "/">] Home
  | [<EndPoint "/counter">] Counter
  | [<EndPoint "/data">] Data
 
/// The Elmish application's model.
type Model =
  { menu: RoutingMenu.Model<Page>
    home: HomePage.Model
    counter: Counter.Model
    books: BooksData.Model
    error: Errors.Model }

/// The Elmish application's update messages.
type Message =
  | MenuMessage of RoutingMenu.Message<Page>
  | HomePageMessage of HomePage.Message
  | CounterMessage of Counter.Message
  | BooksDataMessage of BooksData.Message
  | ErrorsMessage of Errors.Message

/// Connects the routing system to the Elmish application.
let router: Router<Page, Model, Message> =
  Router.infer (RoutingMenu.SetPage >> MenuMessage) (fun (model: Model) -> model.menu.page)

let notifyBookErrors (message: Message) : Message list =
  match message with
  | BooksDataMessage (BooksData.Error e) -> [ (ErrorsMessage(Errors.Error e)) ]
  | _ -> []

let messageRouting: MessageRouter<Message> list = [ notifyBookErrors ]

let homepage =
  HomePage.MakeComponent<Model, Message>(
    ModelWrapper((fun gm lm -> { gm with home = lm }), (fun m -> m.home)),
    MessageWrapper(
      HomePageMessage,
      (fun msg ->
        match msg with
        | HomePageMessage m -> Some m
        | _ -> None),
      messageRouting
    ),
    router.HRef Counter,
    router.HRef Data
  )

let counter =
  Counter.MakeComponent<Model, Message>(
    ModelWrapper((fun gm lm -> { gm with counter = lm }), (fun m -> m.counter)),
    MessageWrapper(
      CounterMessage,
      (fun msg ->
        match msg with
        | CounterMessage m -> Some m
        | _ -> None),
      messageRouting
    )
  )

let books =
  BooksData.MakeComponent<Model, Message>(
    ModelWrapper((fun gm lm -> { gm with books = lm }), (fun m -> m.books)),
    MessageWrapper(
      BooksDataMessage,
      (fun msg ->
        match msg with
        | BooksDataMessage m -> Some m
        | _ -> None),
      messageRouting
    )
  )

let errors =
  Errors.MakeComponent<Model, Message>(
    ModelWrapper((fun gm lm -> { gm with error = lm }), (fun m -> m.error)),
    MessageWrapper(
      ErrorsMessage,
      (fun msg ->
        match msg with
        | ErrorsMessage m -> Some m
        | _ -> None),
      messageRouting
    )
  )

let menu =
  RoutingMenu.MakeComponent<Model, Message, Page>(
    ModelWrapper((fun gm lm -> { gm with menu = lm }), (fun m -> m.menu)),
    MessageWrapper(
      MenuMessage,
      (fun msg ->
        match msg with
        | MenuMessage m -> Some m
        | _ -> None),
      messageRouting
    ),
    Home,
    [ (Home, "Home", router.HRef Home, [ homepage.µ ])
      (Counter, "Counter", router.HRef Counter, [ counter.µ ])
      (Data, "Download data", router.HRef Data, [ books.µ; errors.µ ]) ]
  )

let initModel =
  { menu = menu.InitialModel
    home = homepage.InitialModel
    counter = counter.InitialModel
    books = books.InitialModel
    error = errors.InitialModel }

let update (message: Message) : Services -> Message -> Model -> Model * Cmd<Message> =
  match message with
  | MenuMessage _ -> menu.Update
  | HomePageMessage _ -> homepage.Update
  | CounterMessage _ -> counter.Update
  | BooksDataMessage _ -> books.Update
  | ErrorsMessage _ -> errors.Update

let components =
  [ menu.µ
    homepage.µ
    counter.µ
    books.µ
    errors.µ ]

type MyApp() =
  inherit ProgramComponent<Model, Message>()

  [<Inject>]
  member val HttpClient = Unchecked.defaultof<HttpClient> with get, set

  override this.Program =
    let services: Services =
      { httpClient = this.HttpClient
        jsRuntime = this.JSRuntime }

    Program.mkProgram (fun _ -> initModel, Initialize components) (fun msg -> (update msg) services msg) menu.Show
    |> Program.withRouter router
