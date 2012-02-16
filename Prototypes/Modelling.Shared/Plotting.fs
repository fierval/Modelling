// Learn more about F# at http://fsharp.net
namespace modelling.shared

[<AutoOpen>]
module Plotting =

    open System.Drawing
    open System.Windows.Forms
    open System.Windows.Forms.DataVisualization.Charting
    open Microsoft.FSharp.Metadata
    open System.Reflection
    open MSDN.FSharp.Charting
    open MSDN.FSharp.Charting.ChartStyleExtensions
    open System
    open Microsoft.FSharp.Reflection

    let internal createChartOfType (chartType : string) name (y : #IConvertible seq)  =
        let innerTp = y.GetType().GetGenericArguments().[0]
        let mi = 
            (typeof<FSharpChart>.GetMethods() 
            |> Array.filter(
                fun v -> 
                    v.Name = chartType && v.GetParameters().Length = 1 && v.GetGenericArguments().Length = 1)).[0]
            
        let chart = mi.GetGenericMethodDefinition().MakeGenericMethod([|innerTp|]).Invoke(null, [|y|]) :?> ChartTypes.GenericChart
        chart.Name <- name
        chart

    type ModellingShared() =
        inherit Form()
        
        let mutable chartControl : ChartControl = Unchecked.defaultof<ChartControl>

        member private ms.plot =
            if chartControl = Unchecked.defaultof<ChartControl> 
            then invalidArg "chartControl" "not initialized"
            
            ms.SuspendLayout();
            ms.Controls.Add(chartControl)

            // define form properties
            ms.ClientSize <- new Size(600, 600)
            ms.ResumeLayout(false)
            ms.PerformLayout()
            Application.EnableVisualStyles()
            Application.Run(ms :> Form)

        member ms.Plot 
            (plotData : #seq<#IConvertible> seq, 
                chartType : string,
                ? names : string seq,
                ? title : string,
                ? xTitle : string,
                ? yTitle : string, 
                ? xLimits : float * float, 
                ? yLimits : float * float, 
                ? margin : float32 * float32 * float32 * float32) =

            let marg = defaultArg margin (2.0f, 12.0f, 2.0f, 2.0f)
            let chartTitle = defaultArg title "Chart"
            let xTitle = defaultArg xTitle String.Empty
            let yTitle = defaultArg yTitle String.Empty

            let chartNames = defaultArg names (plotData |> Seq.mapi(fun i v -> "Series " + i.ToString()))
            if (chartNames |> Seq.length) <> (plotData |> Seq.length) then invalidArg "names" "not of the right length"
            
            let plot = plotData |> Seq.zip chartNames
            let mutable chart =  FSharpChart.Combine ([for p in plot -> createChartOfType chartType (fst p) (snd p)])
            chart <- 
                match xLimits with
                | Some (xMin, xMax) -> FSharpChart.WithArea.AxisX(Minimum = xMin, Maximum= xMax, Title = xTitle, MajorGrid = Grid(LineColor = Color.LightGray)) chart
                | None -> chart

            chart <-
                match yLimits with
                | Some (yMin, yMax) -> FSharpChart.WithArea.AxisY(Minimum = yMin, Maximum= yMax, Title = yTitle, MajorGrid = Grid(LineColor = Color.LightGray)) chart
                | None -> chart
                |> FSharpChart.WithMargin marg

            chart <- FSharpChart.WithLegend(InsideArea = false, Alignment = StringAlignment.Center, Docking = Docking.Top) chart
            chart.Title <- StyleHelper.Title(chartTitle, FontSize = 10.0f, FontStyle = FontStyle.Bold)
            chartControl <- new ChartControl(chart, Dock = DockStyle.Fill)
            ms.plot