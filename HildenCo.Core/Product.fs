namespace HildenCo.Core
open System
    type Product () = 
   
           member this.Id = Guid.NewGuid().ToString()
           member val  Slug="" with get, set
           member val  Name="" with get, set
           member val  Description="todo" with get, set
           member val  Price= 0.0m with get, set
           member val  Currency="USD" with get, set
           member val  CreatedOn = DateTime.UtcNow.AddYears(-3)
           member val  LastUpdate = DateTime.UtcNow.AddDays(-17.0)
       
   

