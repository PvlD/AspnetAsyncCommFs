namespace ResponseSvc.Server

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Bolero
open Bolero.Remoting.Server
open Bolero.Server
open ResponseSvc
open Bolero.Templating.Server
open ResponseSvc.Core.Services
open Microsoft.Extensions.Configuration
open MassTransit
open RequestSvc.Core.Config
open ResponseSvc.Core.Consumers
open MassTransit.RabbitMqTransport
open GreenPipes


type Startup(configuration:IConfiguration ) =

    let  cfg = configuration.Get<AppConfig>();

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    member this.ConfigureServices(services: IServiceCollection) =

        services.AddMassTransit( fun  x ->
                     
                          x.AddConsumer<ProductInfoRequestConsumer>() |> ignore

                          x.AddBus(fun context ->
                                             Bus.Factory.CreateUsingRabbitMq(fun c ->
                                                     c.Host(cfg.MassTransit.Host)
                                                     c.ReceiveEndpoint ( cfg.MassTransit.Queue,
                                                                        fun  (e:IRabbitMqReceiveEndpointConfigurator) ->
                                                                                                      e.PrefetchCount <- 16              
                                                                                                      e.UseMessageRetry(fun r -> r.Interval(2, 3000) |> ignore  )
                                                                                                      e.ConfigureConsumer<ProductInfoRequestConsumer>(context)
                                                                                                      
                                                                                                 
                                                                        )
                                             )  
                                     )
                                         

                         
                     ) |> ignore

        services.AddMassTransitHostedService() |> ignore

        services.AddServerSideBlazor() |> ignore
        services.AddTransient<ICatalogSvc, CatalogSvc>() |> ignore

        services
            .AddAuthorization()
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
                .Services
            .AddRemoting<BookService>()
#if DEBUG
            .AddHotReload(templateDir = __SOURCE_DIRECTORY__ + "/../ResponseSvc.Client")
#endif
        |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        
        ignore <| if env.IsDevelopment() then 
        
                    app.UseDeveloperExceptionPage()

                  else 
                   app.UseHsts();



        app
            .UseAuthentication()
            .UseRemoting()
            .UseStaticFiles()
            .UseRouting()
            .UseBlazorFrameworkFiles()
            .UseEndpoints(fun endpoints ->
#if DEBUG
                endpoints.UseHotReload()
#endif
                endpoints.MapFallbackToFile("index.html") |> ignore)
        |> ignore

module Program =

    [<EntryPoint>]
    let main args =
        WebHost
            .CreateDefaultBuilder(args)
            .UseStaticWebAssets()
            .UseStartup<Startup>()
            .Build()
            .Run()
        0
