namespace ResponseSvc.Core.Consumers

open HildenCo.Core
open HildenCo.Core.Contracts
open MassTransit
open ResponseSvc.Core.Services
open System
open System.ComponentModel.DataAnnotations
open System.Threading.Tasks



    /// <summary>
    /// ProductInfoRequestConsumer consumes ProductInfoRequest events
    /// and responds asynchronously with account information.
    /// </summary>
    type ProductInfoRequestConsumer ( svc:ICatalogSvc)=
                interface  IConsumer<ProductInfoRequest> with 

                    member this.Consume( context :ConsumeContext<ProductInfoRequest>) =
                        async {
                            let msg = context.Message;
                            let  slug = msg.Slug;

                            //// a fake delay
                            let  delay = 1000 * (msg.Delay > 0  |> function 
                                                                        |true -> msg.Delay
                                                                        |_-> 1
                                                )
                        
                            do! Async.Sleep (delay)
                        

                            //// get the product from ProductService
                            let  p = svc.GetProductBySlug(slug);

                            //// this responds via the queue to our client
                            do! context.RespondAsync(new ProductInfoResponse(Product = p)) |> Async.AwaitTask
                        
                            return ()  

                        } |> Async.StartAsTask :> Task
