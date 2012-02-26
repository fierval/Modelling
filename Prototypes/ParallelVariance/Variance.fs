namespace modelling.stat

[<AutoOpen>]
module ParallelVariance =

    open System
    open System.Threading
    open System.Threading.Tasks
    open System.Diagnostics
    open System.Collections.Concurrent

    [<StructuredFormatDisplay("{Display}")>]
    [<DebuggerDisplay("{Display}")>]
    type auxValues = 
        {M2: float; mean : float; n : float

        }
        with
            member private a.Display = "M2: " + a.M2.ToString() + "; mean: " + a.mean.ToString() + "; n: " + a.n.ToString()

    let auxValuesOfSet (data : double []) start finish =
        let mutable n = 0.
        let mutable mean = 0.
        let mutable M2 = 0.

        for x = start to finish do
                n <- n + 1.
                let delta = data.[x] - mean
                mean <- mean + delta/n
                if n > 1. then M2 <- M2 + delta * (data.[x] - mean) 

        {M2 = M2; mean = mean; n = float(finish - start + 1)}

    let combineM2s (r1 : auxValues) (r2 : auxValues) =
                            
        let delta = r1.mean - r2.mean
        let deltaSq = delta * delta
        let n = r1.n + r2.n
        let M2 = r1.M2 + r2.M2 + deltaSq * r1.n *r2.n / n
        let mean = (r1.n * r1.mean + r2.n * r2.mean) / n

        {M2 = M2; mean = mean; n = n}

    let Variance2(data : double []) =
        let partition = data.Length / 2
        let finish = data.Length - 1
        let tasks = 
            Task.Factory.StartNew(fun () -> auxValuesOfSet data 0 partition) 
                    :: Task.Factory.StartNew(fun () -> auxValuesOfSet data (partition + 1) finish)
                    :: [] 
                    |> List.toArray

        let results = Task.Factory.ContinueWhenAll(tasks, fun tasks -> tasks |> Array.map(fun (v : Task<auxValues>) -> v.Result)).Result
        let res = combineM2s results.[0] results.[1]
        res.M2 / res.n

    let VarianceForDoesntWork (data : double []) =
        let monitor = new obj()
        let m2 = ref {M2 = 0.; n= 0.; mean = 0.} 

        Parallel.ForEach(
                Partitioner.Create(0, data.Length), 
            (fun () -> {M2 = 0.; n= 0.; mean = 0.}), 
            (fun range state local -> auxValuesOfSet data (fst range) ((snd range) - 1)
                ),
            (fun (local : auxValues) -> lock monitor (fun () -> do m2:= combineM2s local !m2))) |> ignore

        (!m2).M2 / (!m2).n

    let VarianceForTask (data : double []) =
        let monitor = new obj()
        let partitioner = Partitioner.Create(0, data.Length)

        let partitions = partitioner.GetDynamicPartitions()

        let tasks = 
            [| for p in partitions ->
                Task.Factory.StartNew(fun () -> auxValuesOfSet data (fst p) ((snd p) - 1))
            |] 

        let results = Task.Factory.ContinueWhenAll(tasks, fun tasks -> tasks |> Array.map(fun (v : Task<auxValues>) -> v.Result)).Result |> Array.toList

        let res = results |> List.reduce combineM2s
        res.M2 / res.n

    let VarianceForCummul (data : double []) =
        let monitor = new obj()
        let m2list : auxValues list ref = ref []

        Parallel.ForEach(
            Partitioner.Create(0, data.Length), 
            (fun () -> ref []), 
            (fun range state local -> 
                local := auxValuesOfSet data (fst range) ((snd range) - 1) :: !local; local),
            (fun local -> lock monitor (fun () -> m2list := !m2list @ !local))) |> ignore

        let res = !m2list |> List.reduce combineM2s
        res.M2 / res.n
        
    let Variance (data : double []) =
        (auxValuesOfSet data 0 (data.Length - 1)).M2 / (float data.Length)    