namespace RequestSvc.Server

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration

open Bolero.Remoting.Server
open Bolero.Templating.Server

open MassTransit

open HildenCo.Core.Contracts
open RequestSvc.Core.Config




type Startup(configuration:IConfiguration ) =

    let  cfg = configuration.Get<AppConfig>();

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    member this.ConfigureServices(services: IServiceCollection) =

        services.AddMassTransit( fun  x ->
              
                  x.AddBus( fun context -> Bus.Factory.CreateUsingRabbitMq( fun c ->
                  
                      c.Host(cfg.MassTransit.Host);
                      c.ConfigureEndpoints(context);
                  ))
          
                  x.AddRequestClient<ProductInfoRequest>();
                  
              ) |> ignore

        services.AddMassTransitHostedService() |> ignore

        //services.AddMvc() |> ignore
        services.AddServerSideBlazor() |> ignore
        services
            .AddAuthorization()
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
                .Services
            .AddRemoting<ProductsSetvice>()
#if DEBUG
            .AddHotReload(templateDir = __SOURCE_DIRECTORY__ + "/../RequestSvc.Client")
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
                //endpoints.MapControllers() |> ignore
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
