using modelling.stat;
using System;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParallelVarianceCalc
{
    class Program
    {
        static void Main(string[] args)
        {
            var rnd = new MersenneTwister();
            double[] data = new double[int.Parse(args[0])];
            //Enumerable.Range(1, int.Parse(args[0])).AsParallel().Select(i => Normal.Sample(rnd, 4.5, 2.0)).ToArray();

            Parallel.For(0, int.Parse(args[0]), i => data[i] = Normal.Sample(rnd, 4.5, 2.0));

            var sw = new Stopwatch();
            char c = ' ';
            while (c != 'q')
            {
                sw.Start();
                double single = Math.Round(ParallelVariance.Variance(data), 4, MidpointRounding.AwayFromZero);
                sw.Stop();
                var elapsedSingle = sw.ElapsedMilliseconds;
  
                sw.Reset();
                sw.Start();
                double actual = Math.Round(ParallelVariance.Variance2(data), 4, MidpointRounding.AwayFromZero);
                sw.Stop();
                var elapsedVariance = sw.ElapsedMilliseconds;
                sw.Reset();

                sw.Start();
                double loop = Math.Round(ParallelVariance.VarianceForCummul(data), 4, MidpointRounding.AwayFromZero);
                sw.Stop();
                var elapsedVarianceFor = sw.ElapsedMilliseconds;
                sw.Reset();

                sw.Start();
                double loopTask = Math.Round(ParallelVariance.VarianceForTask(data), 4, MidpointRounding.AwayFromZero);
                sw.Stop();
                var elapsedVarianceForTask = sw.ElapsedMilliseconds;
                sw.Reset();

                sw.Start();
                double expected = Math.Round(Statistics.Variance(data), 4, MidpointRounding.AwayFromZero);
                sw.Stop();

                var elapsedPackageVariance = sw.ElapsedMilliseconds;

                Console.WriteLine("MathNet: {0}, Local: {1}, Parallel: {2}, Parallel.ForEach: {3}, Parallel.ForEachTasks: {4}", 
                    expected, single, actual, loop, loopTask);
                Console.WriteLine("Elapsed MathNet: {0}ms, Elapsed local: {1}ms, parallel: {2}ms, parallel.ForEach: {3}ms, parallel.ForEachTask: {4}", 
                    elapsedPackageVariance, elapsedSingle, elapsedVariance, elapsedVarianceFor, elapsedVarianceForTask);
                c = Console.ReadKey(true).KeyChar;
            }
        }
    }
}
