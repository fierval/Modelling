namespace modelling.shared

[<AutoOpen>]
module Retry =
    open System.Threading
    open System

    type RetryParams = {
        maxRetries : int; waitBetweenRetries : int
        }

    let defaultRetryParams = {maxRetries = 3; waitBetweenRetries = 1000}

    type RetryMonad<'a> = RetryParams -> 'a
    let rm<'a> (f : RetryParams -> 'a) : RetryMonad<'a> = f

    let internal retryFunc<'a> (f : RetryMonad<'a>) =
        rm (fun retryParams -> 
            let rec execWithRetry f i e =
                match i with
                | n when n = retryParams.maxRetries -> raise e
                | _ -> 
                    try
                        Console.WriteLine("Attempt {0}", i + 1)
                        f retryParams
                    with 
                    | e -> Thread.Sleep(retryParams.waitBetweenRetries); execWithRetry f (i + 1) e
            execWithRetry f 0 (Exception())
            ) 

    
    type RetryBuilder() =
        
        member this.Bind (p : RetryMonad<'a>, f : 'a -> RetryMonad<'b>)  =
            rm (fun retryParams -> 
                let value = retryFunc p retryParams
                Console.WriteLine("Result: {0}. MaxRetries: {1}. Wait: {2}.", value, retryParams.maxRetries, retryParams.waitBetweenRetries)
                f value retryParams                
            )

        member this.Return (x : 'a) = fun defaultRetryParams -> x

        member this.Run(m : RetryMonad<'a>) = m

        member this.Delay(f : unit -> RetryMonad<'a>) = f ()

    let retry = RetryBuilder()