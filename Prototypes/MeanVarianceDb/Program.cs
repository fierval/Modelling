using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModelDataLayer;
using modelling.meanvariance;
using Microsoft.FSharp.Collections;

namespace MeanVarianceDb
{
    class Program
    {
        static void Main(string[] args)
        {
            var rm = RiskMinimizationFormulation.CreateRiskMinimizationFromDb ();
            var range = ListModule.OfSeq(Enumerable.Range(50, 120).Where(e => e % 5 == 0).Select(e => (double)e / 1000D));

            rm.ChartOptimalWeights(range, new string[] { "Australia", "Austria", "Belgium", "Canada" });
        }
    }
}
