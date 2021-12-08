module BoleroSample.Client.BooksData

open System
open System.Net.Http.Json
open Elmish
open Bolero.Html
open Component

type Book =
  { title: string
    author: string
    publishDate: DateTime
    isbn: string }

type Model = { books: Book [] option }

type Message =
  | GetBooks
  | GotBooks of Book []
  | Error of exn

type MakeComponent<'globalModel, 'globalMessage>
  (
    mdw: ModelWrapper<Model, 'globalModel>,
    msw: MessageWrapper<Message, 'globalMessage>
  ) =
  inherit Component<Model, Message, 'globalModel, 'globalMessage>(mdw, msw)
  override this.InitialModel = { books = None }
  override this.InitialMessage = Some GetBooks

  override this.OnUpdate (services: Services) (message: Message) (model: Model) : Model * Cmd<Message> =
    match message with
    | GetBooks ->
      let getBooks () =
        services.httpClient.GetFromJsonAsync<Book []>("/books.json")

      services.Log("Loading books...")
      { model with books = None }, Cmd.OfTask.either getBooks () GotBooks Error

    | GotBooks books ->
      services.Log("Got books!")
      { model with books = Some books }, Cmd.none

    | Error e ->
      services.Log($"ERROR {e.Message}")
      model, Cmd.none

  override this.View
    (model: Model)
    (dispatch: Message -> unit)
    (showSubView: SubViewDisplay<'globalModel, 'globalMessage>)
    =
    concat [ h1 [ attr.``class`` "title" ] [
               text "Download data "
               button [ attr.``class`` "button"
                        on.click (fun _ -> dispatch GetBooks) ] [
                 text "Reload"
               ]
             ]
             table [ attr.``class`` "table is-fullwidth" ] [
               thead [] [
                 tr [] [
                   th [] [ text "Title" ]
                   th [] [ text "Author" ]
                   th [] [ text "Published" ]
                   th [] [ text "ISBN" ]
                 ]
               ]
               tbody [] [
                 cond model.books
                 <| function
                   | None ->
                     tr [] [
                       td [ attr.colspan 4 ] [
                         text "Downloading book list..."
                       ]
                     ]
                   | Some books ->
                     forEach books
                     <| fun book ->
                          tr [] [
                            td [] [ text book.title ]
                            td [] [ text book.author ]
                            td [] [
                              text (book.publishDate.ToString("yyyy-MM-dd"))
                            ]
                            td [] [ text book.isbn ]
                          ]
               ]
             ] ]
