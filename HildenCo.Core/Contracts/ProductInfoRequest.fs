namespace HildenCo.Core.Contracts


 type ProductInfoRequest()  = 
              member val  Slug="" with get, set

              // simulate a fake delay from the remote service
              member val  Delay=0 with get, set


