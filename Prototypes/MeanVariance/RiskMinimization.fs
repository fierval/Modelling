// Learn more about F# at http://fsharp.net
namespace modelling.meanvariance

[<AutoOpen>]
module RiskMinimizationFormulation =
    
    open System
    open MathNet.Numerics.FSharp
    open MathNet.Numerics.LinearAlgebra.Double
    open modelling.shared

    type RiskMinimization(expected : Vector, correlations : Matrix, stdDeviations : Vector) =
        do
            if correlations.RowCount <> correlations.ColumnCount 
            then 
                invalidArg "variances" "expected square matrix" 

            if expected.Count <> correlations.RowCount
            then
                invalidArg "expected" "expectations matrix must have the same length as variance matrix dimensions" 

        let variances = 
            correlations 
            |> Matrix.mapi 
                (fun i j value -> 
                    let mutable v = value
                    if correlations.[i,j] = 0.0 
                    then 
                        v <- correlations.[j, i]
                    stdDeviations.[i] * stdDeviations.[j] * v)
        let n = expected.Count
        let i = vector (List.init n (fun i -> 1.))

        let variancesInv = variances.Inverse()
        let a = i * variancesInv * i
        let b = i * variancesInv * expected
        let c = expected * variancesInv * expected
        let denom = 1. / (a * c - b*b)
        let g = denom * (variancesInv * (c * i - b * expected))
        let h = denom * (variancesInv * (a * expected - b * i))

        member rm.ComputeOptimal (expectation : float) =
            g + h * expectation

        member rm.ChartOptimalWeights (expectations : float list) (names : string seq) =
            let data = matrix(expectations |> List.map (fun v -> (rm.ComputeOptimal v |> Vector.toList)))

            let plotData =  seq {for i in 0 .. data.ColumnCount - 1 -> data.Column(i).ToArray() |> Array.toList} 
            Charting.Plot(
                "Line", 
                plotData,
                plotX = (expectations |> List.map(fun v -> v * 100.)), 
                xTitle = "Expected Return",
                yTitle = "Asset Weight",
                xLimits = (5., 12.),
                yLimits = (-2.0, 3.0),
                seriesNames = names,
                title = "Risk Minimization Model")
        
        member rm.ComputeStandardDeviation (expectation : float) =
            let weights = rm.ComputeOptimal expectation
            Math.Sqrt(weights * variances * weights)

        member rm.ChartStandardDeviation (expectations : float list) =
            let data = (expectations |> List.map(fun v -> 100.0 * rm.ComputeStandardDeviation v))
            Charting.Plot(
                "Line",
                [expectations |> List.map(fun v -> v * 100.)],
                plotX = data,
                yLimits = (5.0, 12.0),
                xLimits = (10.0, 55.0),
                xTitle = "Standard Deviation",
                yTitle = "Expectation",
                seriesNames = ["Mean-Variance Efficient Frontier"],
                title = "Standard Deviation Chart")                   