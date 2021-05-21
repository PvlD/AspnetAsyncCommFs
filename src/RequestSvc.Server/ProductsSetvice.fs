namespace RequestSvc.Server



open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.AspNetCore.Hosting
open MassTransit
open Bolero
open Bolero.Remoting
open Bolero.Remoting.Server
open RequestSvc
open HildenCo.Core.Contracts
open Microsoft.Extensions.DependencyInjection

type ProductsSetvice(ctx: IRemoteContext, env: IWebHostEnvironment )=
    inherit RemoteHandler<Client.Main.ProductsSetvice>()

    override this.Handler =
        {
            getBySlug =   fun ( slug:string  , timeout:int) -> async {

                let client = ctx.HttpContext.RequestServices.GetService<IRequestClient<ProductInfoRequest>>();

                use   request = client.Create(new ProductInfoRequest(Slug = slug, Delay = timeout) )
                let! response = request.GetResponse<ProductInfoResponse>() |> Async.AwaitTask
                return response.Message.Product;

                //return  new HildenCo.Core.Product()
            }
        }

