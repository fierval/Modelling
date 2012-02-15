// Learn more about F# at http://fsharp.net
namespace model.meanvariance

[<AutoOpen>]
module RiskMinimizationFormulation =
    
    open System
    open Microsoft.FSharp.Math
    open System.Drawing
    open System.Windows.Forms
    open System.Windows.Forms.DataVisualization.Charting
    open Plotting

    open MSDN.FSharp.Charting
    open MSDN.FSharp.Charting.ChartStyleExtensions

    type RiskMinimization(expected : Vector<float>, correlations : Matrix<float>, stdDeviations : Vector<float>) =
        inherit ModellingShared()

        let mutable chartControl : ChartControl = Unchecked.defaultof<ChartControl>
        let variances = 
            correlations 
            |> Matrix.mapi 
                (fun i j value -> 
                    let mutable v = value
                    if correlations.[i,j] = 0.0 
                    then 
                        v <- correlations.[j, i]
                    stdDeviations.[i] * stdDeviations.[j] * v)
        let n = expected.Length
        let i = Vector.create n 1.

        do
            if fst variances.Dimensions <> snd variances.Dimensions 
            then 
                invalidArg "expected square matrix" |> ignore

            if expected.Length <> fst variances.Dimensions
            then
                invalidArg "expectations matrix must have the same length as variance matrix dimensions" |> ignore

        let i' = i.Transpose
        let variances' = variances.Transpose
        let expected' = expected.Transpose

        let a = i' * variances' * i
        let b = i' * variances' * expected
        let c = expected' * variances' * expected
        let denom = 1. / (a * c - b*b)
        let g = denom * (variances' * (c * i - b * expected))
        let h = denom * (variances' * (a * expected - b * i))

        member rm.ComputeOptimal (expectation : float) =
            g + h * expectation    

        member rm.ChartOptimalWeights (expectations : float list) =
            let data = expectations |> List.map (fun v -> rm.ComputeOptimal v |> Vector.toArray |> List.ofArray) |> Matrix.ofList

            let n = snd data.Dimensions - 1
            let plotData = seq {for i in 0 .. n -> data.Column(i).ToArray() |> Seq.ofArray}
            rm.Plot(plotData, "Line", (5.0, 12.0), (-2.0, 2.0))



                    


