module ResponseSvc.Client.Main

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting
open Bolero.Remoting.Client
open Bolero.Templating.Client

open Microsoft.JSInterop

open HildenCo.Core

/// Routing endpoints definition.
type Page =
    | [<EndPoint "/" >] Home
    | [<EndPoint "/ResponseSvc">] ResponseSvc 
    with static member 
            toTitle = 
                function                
                    | Page.Home | Page.ResponseSvc -> "Home Page" + "- ResponseSvc"
                    |_-> "ResponseSvc"

/// The Elmish application's model.
type Model =
    {
        page: Page
        allProducts: Product list  option 
        error: string option
    }


let initModel =
    {
        page = Home
        allProducts = None
        error = None
    }

/// Remote service definition.
type ProductsSetvice =
    {
     allProducts : unit -> Async< Product  list >
    }
    interface IRemoteService with
        member this.BasePath = "/Products"

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | SetTitle of string
    | GotAllProducts of Product list
    | GetAllProducts 
    | Error of exn
    | ClearError

let update (js: IJSRuntime)  remote message model =

    match message with
    | SetPage page ->
        { model with page = page }, Cmd.ofMsg (SetTitle (Page.toTitle page) )

    | SetTitle t ->
               let cmd = Cmd.OfJS.attempt  js "JI.setTitle" [| t |]  Error
               model , cmd

    | GetAllProducts ->
        let cmd = Cmd.OfAsync.either remote.allProducts () GotAllProducts Error
        { model with allProducts = None}, cmd
    | GotAllProducts  allProducts ->
        { model with allProducts = Some allProducts }, Cmd.none
    | Error (RemoteException ex) ->
        { model with error = Some ("RemoteException: " + ex.ToString() + "\n Content:"  + ex.Content.ReadAsStringAsync().Result  ) ; }, Cmd.none
    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none
    | ClearError ->
        { model with error = None }, Cmd.none

/// Connects the routing system to the Elmish application.
let router = Router.infer SetPage (fun model -> model.page)

type Main = Template<"wwwroot/main.html">

let homePage (model:Model) dispatch =
    Main.Home()
        .Rows(cond model.allProducts  <| function
        | None ->
            Main.EmptyData().Elt()
        | Some products ->
            forEach products <| fun product ->
                tr [] [
                    td [] [text product.Slug]
                    td [] [text product.Name]
                    td [] [text product.Description]
                    td [] [text (product.Currency + " " +  product.Price.ToString())]
                ])
        .Elt()


let menuItem (model: Model) (page: Page) (text: string) =
    Main.MenuItem()
        .Active(if model.page = page then "is-active" else "")
        .Url(router.Link page)
        .Text(text)
        .Elt()

let view model dispatch =
    Main()
        .Menu(concat [
            menuItem model Home "Home"
            menuItem model ResponseSvc "ResponseSvc"
        ])
        .Body(
            cond model.page <| function
            | Home -> homePage model dispatch
            | ResponseSvc -> homePage model dispatch

        )
        .Error(
            cond model.error <| function
            | None -> empty
            | Some err ->
                Main.ErrorNotification()
                    .Text(err)
                    .Hide(fun _ -> dispatch ClearError)
                    .Elt()
        )
        .Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        let productsSetviceice = this.Remote<ProductsSetvice>()
        let update = update this.JSRuntime productsSetviceice
        Program.mkProgram (fun _ -> initModel, Cmd.ofMsg GetAllProducts) update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif
