using modelling.meanvariance;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MathNet.Numerics.LinearAlgebra.Double;

using Microsoft.FSharp.Collections;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace ModelTests
{
    
    
    /// <summary>
    ///This is a test class for RiskMinimizationFormulation_RiskMinimizationTest and is intended
    ///to contain all RiskMinimizationFormulation_RiskMinimizationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class RiskMinimizationFormulation_RiskMinimizationTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        [TestMethod]
        public void ChartWeights()
        {
            Vector expected =  new DenseVector (new double [] { 0.079, 0.079, 0.09, 0.071 });

            Matrix correlations = new DenseMatrix(new double[,] { { 1.0, 0F, 0F, 0F }, { 0.24, 1.0, 0F, 0F }, { 0.25, 0.47, 1.0, 0F }, { 0.22, 0.14, 0.25, 1.0 } });

            Vector stdDeviations = new DenseVector(new double[] { 0.195, 0.182, 0.183, 0.165 });
            RiskMinimizationFormulation.RiskMinimization model = new RiskMinimizationFormulation.RiskMinimization(expected, correlations, stdDeviations);
            
            var range = ListModule.OfSeq(Enumerable.Range(50, 120).Where(e => e % 5 == 0).Select(e => (double)e / 1000D));

            model.ChartOptimalWeights(range, new string [] {"Australia", "Austria", "Belgium", "Canada"});
        }

        [TestMethod]
        public void ChartStd()
        {
            Vector expected = new DenseVector(new double[] { 0.079, 0.079, 0.09, 0.071 });

            Matrix correlations = new DenseMatrix(new double[,] { { 1.0, 0F, 0F, 0F }, { 0.24, 1.0, 0F, 0F }, { 0.25, 0.47, 1.0, 0F }, { 0.22, 0.14, 0.25, 1.0 } });

            Vector stdDeviations = new DenseVector(new double[] { 0.195, 0.182, 0.183, 0.165 });
            RiskMinimizationFormulation.RiskMinimization model = new RiskMinimizationFormulation.RiskMinimization(expected, correlations, stdDeviations);

            var range = ListModule.OfSeq(Enumerable.Range(50, 120).Where(e => e % 5 == 0).Select(e => (double)e / 1000D));

            model.ChartStandardDeviation(range);

        }

    }
}
