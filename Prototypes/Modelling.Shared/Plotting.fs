// Learn more about F# at http://fsharp.net
namespace modelling.shared

[<AutoOpen>]
module Plotting =

    open System.Drawing
    open System.Windows.Forms
    open System.Windows.Forms.DataVisualization.Charting
    open Microsoft.FSharp.Metadata
    open Microsoft.FSharp.Reflection
    open System.Reflection
    open MSDN.FSharp.Charting
    open MSDN.FSharp.Charting.ChartStyleExtensions
    open System
    open Microsoft.FSharp.Reflection

    let internal createChartOfType (chartType : string) name (y : seq<#IConvertible * #IConvertible>)  =
        let innerTps = FSharpType.GetTupleElements(y.GetType().GetGenericArguments().[0])
        let mi = 
            (typeof<FSharpChart>.GetMethods() 
            |> Array.filter(
                fun v -> 
                    v.Name = chartType && v.GetParameters().Length = 1 && v.GetGenericArguments().Length = 2)).[0]
            
        let chart = mi.GetGenericMethodDefinition().MakeGenericMethod(innerTps).Invoke(null, [|y|]) :?> ChartTypes.GenericChart
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
            (
            plotX : list<#IConvertible>,
            plotData : list<#IConvertible> seq, 
            chartType : string,
            ? seriesNames : string seq,
            ? title : string,
            ? xTitle : string,
            ? yTitle : string, 
            ? xLimits : float * float, 
            ? yLimits : float * float, 
            ? margin : float32 * float32 * float32 * float32) =

            let marg = defaultArg margin (4.0f, 12.0f, 4.0f, 4.0f)
            let chartTitle = defaultArg title "Chart"
            let xTitle = defaultArg xTitle String.Empty
            let yTitle = defaultArg yTitle String.Empty

            let chartNames = defaultArg seriesNames (plotData |> Seq.mapi(fun i v -> "Series " + i.ToString()))
            if (chartNames |> Seq.length) <> (plotData |> Seq.length) then invalidArg "names" "not of the right length"
            
            // zip up the relevant information together
            // x-values go with every y-series values in a tuple
            // series names gets zipped with every sequence of (x, y) tuples: (name, (x, y))
            let plot = plotData |> Seq.map(fun s -> List.zip plotX s) |> Seq.zip chartNames

            //create the chart
            let mutable chart =  
                FSharpChart.Combine ([for p in plot -> createChartOfType chartType (fst p) (snd p)])
            
            //add x and y limits
            chart <- 
                match xLimits with
                | Some (xMin, xMax) -> FSharpChart.WithArea.AxisX(Minimum = xMin, Maximum= xMax, MajorGrid = Grid(LineColor = Color.LightGray)) chart
                | None -> FSharpChart.WithArea.AxisX(MajorGrid = Grid(LineColor = Color.LightGray)) chart

            chart <-
                match yLimits with
                | Some (yMin, yMax) -> FSharpChart.WithArea.AxisY(Minimum = yMin, Maximum= yMax, MajorGrid = Grid(LineColor = Color.LightGray)) chart
                | None -> FSharpChart.WithArea.AxisY(MajorGrid = Grid(LineColor = Color.LightGray)) chart
                //... and margin
                |> FSharpChart.WithMargin marg

            //set the titles
            chart.Area.AxisX.Title <- xTitle
            chart.Area.AxisY.Title <- yTitle

            //add legend
            chart <- 
                FSharpChart.WithLegend(InsideArea = false, Alignment = StringAlignment.Center, Docking = Docking.Top) chart

            //add title
            chart.Title <- StyleHelper.Title(chartTitle, FontSize = 10.0f, FontStyle = FontStyle.Bold)

            //create the control
            chartControl <- new ChartControl(chart, Dock = DockStyle.Fill)

            ms.plot