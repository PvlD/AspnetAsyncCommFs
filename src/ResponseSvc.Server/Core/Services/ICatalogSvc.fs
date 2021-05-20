namespace ResponseSvc.Core.Services

open HildenCo.Core

type ICatalogSvc =
    
    abstract member GetProductBySlug : string ->  Product 
    abstract member GetAllProducts   :  unit ->  Product list 

