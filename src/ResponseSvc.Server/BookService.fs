namespace ResponseSvc.Server

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Bolero
open Bolero.Remoting
open Bolero.Remoting.Server
open ResponseSvc
open ResponseSvc.Core.Services

type BookService(ctx: IRemoteContext, env: IWebHostEnvironment) =
    inherit RemoteHandler<Client.Main.ProductsSetvice>()


    override this.Handler =
        {

            allProducts = fun () -> async {

                 let svc = ctx.HttpContext.RequestServices.GetService<ICatalogSvc>()
                 return svc.GetAllProducts()

            }

        }
