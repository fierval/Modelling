using modelling.stat;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.FSharp.Collections;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;
using Microsoft.FSharp.Core;
using MathNet.Numerics.Statistics;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace ModelTests
{
    
    
    /// <summary>
    ///This is a test class for ParallelVariance_ParallelVarianceTest and is intended
    ///to contain all ParallelVariance_ParallelVarianceTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ParallelVariance_ParallelVarianceTest
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


        /// <summary>
        ///A test for Variance
        ///</summary>
        [TestMethod()]
        public void VarianceTest()
        {
            var rnd = new MersenneTwister();
            double [] data = Enumerable.Range(1, 10000000).AsParallel().Select(i => Normal.Sample(rnd, 4.5, 2.0)).ToArray();

            double actual = Math.Round(ParallelVariance.Variance2(data), 3, MidpointRounding.AwayFromZero);
            double expected = Math.Round(Statistics.Variance(data), 3, MidpointRounding.AwayFromZero);

            Assert.AreEqual(expected, actual);
        }
    }
}
