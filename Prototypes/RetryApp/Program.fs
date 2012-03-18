// Learn more about F# at http://fsharp.net

open System
open modelling.shared

[<EntryPoint>]
let Main(args) =
    
    let fn1 (x:float) (y:float) = rm (fun rp -> x * y)
    let fn2 (x:float) (y:float) = rm (fun rp -> if y = 0. then raise (invalidArg "y" "cannot be 0") else x / y)

    try
        let x = 
            (retry {
                let! a = fn1 7. 5.
                let! b = fn1 a 10.
                return b
            }) defaultRetryParams 

        printfn "first retry: %f" x

        let retryParams = {maxRetries = 5; waitBetweenRetries = 100}

        let ym = 
            retry {
                let! a = fn1 7. 5.
                let! b = fn1 a a
                let! c = fn2 b 0.
                return c
            }

        let y = ym retryParams
        0
    with
        e -> Console.WriteLine(e.Message); 1
        