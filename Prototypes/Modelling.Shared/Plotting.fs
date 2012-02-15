// Learn more about F# at http://fsharp.net

module Plotting

    open System.Drawing
    open System.Windows.Forms
    open System.Windows.Forms.DataVisualization.Charting
    open Microsoft.FSharp.Metadata
    open System.Reflection
    open MSDN.FSharp.Charting
    open MSDN.FSharp.Charting.ChartStyleExtensions
    open System
    open Microsoft.FSharp.Reflection

    let createChartOfType (chartType : string) (y : #IConvertible seq)  =
        let innerTp = typeof<seq<_>>.MakeGenericType( [|y.GetType() |])
        let mi = typeof<FSharpChart>.GetMethod(chartType, [|innerTp|]) 
        mi.Invoke(null, [|y|]) :?> ChartTypes.GenericChart

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
            (plotData : #IConvertible seq seq, 
                chartType : string, 
                ?xLimits : float * float, 
                ?yLimits : float * float, 
                ?margin : float32 * float32 * float32 * float32) =

            let mutable chart = FSharpChart.Combine ([for p in plotData -> createChartOfType chartType p ])
            let marg = defaultArg margin (5.0f, 5.0f, 5.0f, 5.0f)
            match xLimits with
            | Some (xMin, xMax) -> chart <- FSharpChart.WithArea.AxisX(Minimum = xMin, Maximum= xMax, MajorGrid = Grid(LineColor = Color.LightGray)) chart
            | None -> ()

            match yLimits with
            | Some (yMin, yMax) -> chart <- FSharpChart.WithArea.AxisY(Minimum = yMin, Maximum= yMax, MajorGrid = Grid(LineColor = Color.LightGray)) chart
            | None -> ()
            
            chart <- (chart |> FSharpChart.WithMargin marg)
            chartControl <- new ChartControl(chart, Dock = DockStyle.Fill)
            ms.plot