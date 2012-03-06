using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelDataLayer
{
    public sealed class MeanVarianceData : IDisposable
    {
        MeanVarianceEntities entities;
        List<Market> markets;

        public MeanVarianceData()
        {
            this.entities = new MeanVarianceEntities();
            this.markets = entities.Markets.ToList();
        }
        
        public IEnumerable<string> GetMarketNames()
        {
            return markets.Select(m => m.Name).ToArray();
        }

        public IEnumerable<double> GetExpected()
        {
            return markets.Select(m => m.Expected).ToArray();
        }

        public IEnumerable<double> GetStds()
        {
            return markets.Select(m => m.Variance).ToArray();
        }


        public double [,] GetCorelations()
        {
            double[,] result = new double[this.markets.Count, this.markets.Count];
            result.Initialize();
            var corelations = this.entities.Corelations.ToList();

            foreach (var c in corelations)
            {
                result[c.ID1 - 2, c.ID2 - 2] = c.Corelation1;
            }

            return result;
        }

        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }

        ~MeanVarianceData()
        {
            Close();
        }

        void Close()
        {
            this.entities.Dispose();
        }
    }
}
