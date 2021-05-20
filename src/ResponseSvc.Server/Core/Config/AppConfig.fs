namespace RequestSvc.Core.Config

type AppConfig () = 
            member val MassTransit : MassTransitConfig  = Unchecked.defaultof<MassTransitConfig  > with get, set
        

