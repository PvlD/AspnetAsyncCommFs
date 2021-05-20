module RequestSvc.Client.Main

open System
open Microsoft.JSInterop



open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting
open Bolero.Remoting.Client
open Bolero.Templating.Client




/// Routing endpoints definition.
type Page =
    | [<EndPoint "/" >] Home
    | [<EndPoint "/RequestSvc">] RequestSvc 
    with static member 
            toTitle = 
                function                
                    | Page.Home | Page.RequestSvc -> "Home Page" + "- RequestSvc"
                    |_-> "RequestSvc"
                


/// The Elmish application's model.
type Model =
    {
        page: Page
        slug:string
        timeout:int

        sendRequestFailed: bool


        productData: string

        error: string option

    }


let initModel =
    {
        page = Home
        slug=""
        timeout=1


        error = None

        sendRequestFailed = false

        productData=""
        
    }

/// Remote service definition.


type ProductsSetvice =
    {
     getBySlug : string * int -> Async<HildenCo.Core.Product>
    }
    interface IRemoteService with
        member this.BasePath = "/Products"


/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | SetTitle of string
    | SetProductSlug of string
    | SetFakeTimeout of int
    | SendRequest
    | RecvRequest of HildenCo.Core.Product
    | Error of exn
    | ClearError

let update (js: IJSRuntime)  (remoteProducts:ProductsSetvice)  message model =
    match message with
    | SetPage page ->
        
        { model with page = page }, Cmd.ofMsg (SetTitle (Page.toTitle page) )

    | SetTitle t ->
        let cmd = Cmd.OfJS.attempt  js "JI.setTitle" [| t |]  Error
        model , cmd
        
    | SetProductSlug v ->
        { model with slug = v;  }, Cmd.none

    | SendRequest ->
       model, Cmd.OfAsync.either remoteProducts.getBySlug (model.slug, model.timeout)    RecvRequest Error 

    | RecvRequest  v ->
         let productData =   System.Text.Json.JsonSerializer.Serialize(v);    
         { model with productData = productData;  }, Cmd.none
        
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
        .ProductSlug(model.slug , fun v -> dispatch (SetProductSlug v))
        .FakeTimeout(model.timeout.ToString(), fun v -> dispatch (SetFakeTimeout (Int32.Parse(v))) )
        .Submit(fun _ -> dispatch SendRequest)
        .ErrorNotification(
                   cond model.sendRequestFailed <| function
                   | false -> empty
                   | true ->
                       Main.ErrorNotification()
                           .HideClass("is-hidden")
                           .Text("SendRequest failied.")
                           .Elt()
               )
        .ProductData(model.productData)
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
            menuItem model RequestSvc "RequestSvc"
        ])
        .Body(
            cond model.page <| function
            | RequestSvc -> homePage model dispatch
            | Home -> homePage model dispatch
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
        let productsSetvice = this.Remote<ProductsSetvice>()
        let update = update this.JSRuntime productsSetvice 
        Program.mkProgram (fun _ -> initModel, Cmd.none) update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif
