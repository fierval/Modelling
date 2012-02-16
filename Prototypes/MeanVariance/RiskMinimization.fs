// Learn more about F# at http://fsharp.net
namespace modelling.meanvariance

[<AutoOpen>]
module RiskMinimizationFormulation =
    
    open System
    open MathNet.Numerics.FSharp
    open Microsoft.FSharp.Math
    open MathNet.Numerics.LinearAlgebra.Double
    open System.Drawing
    open System.Windows.Forms
    open System.Windows.Forms.DataVisualization.Charting
    open modelling.shared

    open MSDN.FSharp.Charting
    open MSDN.FSharp.Charting.ChartStyleExtensions

    module SimpleMatrix = Microsoft.FSharp.Math.Matrix
    module SimpleVector = Microsoft.FSharp.Math.Vector

    type RiskMinimization(expected : Vector<float>, correlations : Matrix<float>, stdDeviations : Vector<float>) =
        inherit ModellingShared()

        let variances = 
            correlations 
            |> SimpleMatrix.mapi 
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
                invalidArg "variances" "expected square matrix" 

            if expected.Length <> fst variances.Dimensions
            then
                invalidArg "expected" "expectations matrix must have the same length as variance matrix dimensions" 

        let i' = i.Transpose
        let variancesInv = (new DenseMatrix(variances |> Matrix.toArray2D)).Inverse().ToArray() |> SimpleMatrix.ofArray2D
        let expected' = expected.Transpose
        let a = i' * variancesInv * i
        let b = i' * variancesInv * expected
        let c = expected' * variancesInv * expected
        let denom = 1. / (a * c - b*b)
        let g = denom * (variancesInv * (c * i - b * expected))
        let h = denom * (variancesInv * (a * expected - b * i))

        member rm.ComputeOptimal (expectation : float) =
            g + h * expectation    

        member rm.ChartOptimalWeights (expectations : float list) (names : string seq) =
            let data = expectations |> List.map (fun v -> rm.ComputeOptimal v |> SimpleVector.toArray |> List.ofArray) |> SimpleMatrix.ofList

            let n = snd data.Dimensions - 1
            let plotData =  seq {for i in 0 .. n -> data.Column(i).ToArray() |> List.ofArray |> List.zip expectations} 
            rm.Plot(
                plotData, 
                "Line", 
                xTitle = "Expected Return (%)",
                yTitle = "Asset Weight",
                xLimits = (5.0, 12.0), 
                yLimits = (-2.0, 2.0), 
                seriesNames = names,
                title = "Risk Minimization Model")
        
        member rm.ComputeStandardDeviation (expectation : float) =
            let weights = rm.ComputeOptimal expectation
            Math.Sqrt(weights.Transpose * variances * weights)

        member rm.ChartStandardDeviation (expectations : float list) =
            let data = (expectations |> List.map(fun v -> 100.0 * rm.ComputeStandardDeviation v) |> List.zip expectations) :: []
            rm.Plot(
                data,
                "Line",
                xLimits = (5.0, 12.0), 
                yTitle = "Standar Deviation",
                xTitle = "Expectation",
                title = "Standard Deviation Chart"
                    )