// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Linq;
using COIN;
using Sonnet;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace SonnetTest
{
    [TestClass]
    public class Sonnet_CoinNativeTests
    {
        /// <summary>
        /// Gets or sets the directory to be used for coin-or-sample.
        /// Can be set directly or via environment variable SONNET_SAMPLEDIR.
        /// If this folder is used depends on the test. See there.
        /// </summary>
        public string SampleDir { get; set; }

        /// <summary>
        /// Gets or sets the directory to be used for coin-or-miplib3.
        /// Can be set directly or via environment variable SONNET_MIPLIBDIR.
        /// If this folder is used depends on the test. See there.
        /// </summary>
        public string MipLibDir { get; set; }

        /// <summary>
        /// Run native Coin tests for Cbc, Clp, Osi etc. 
        /// Set SampleDir directly, or via environment variable SONNET_SAMPLEDIR.
        /// Same for MipLibDir via SONNET_MIPLIBDIR
        /// </summary>
        public Sonnet_CoinNativeTests()
        {
            SampleDir = Environment.GetEnvironmentVariable("SONNET_SAMPLEDIR");
            if (SampleDir != null) SampleDir = SampleDir.Trim('"').Trim('\'');
            MipLibDir = Environment.GetEnvironmentVariable("SONNET_MIPLIBDIR");
            if (MipLibDir != null) MipLibDir = MipLibDir.Trim('"').Trim('\'');

#if NETCOREAPP
            // By default, the executable will be one directory deeper in NET6 compared to NET4
            SampleDir = SampleDir ?? "..\\..\\..\\..\\..\\..\\..\\..\\Data\\sample";
            MipLibDir = MipLibDir ?? "..\\..\\..\\..\\..\\..\\..\\..\\Data\\miplib3";
#else
            SampleDir = SampleDir ?? "..\\..\\..\\..\\..\\..\\..\\Data\\sample";
            MipLibDir = MipLibDir ?? "..\\..\\..\\..\\..\\..\\..\\Data\\miplib3";
#endif

    }
    /// <summary>
    /// Runs the Cbc native gams test.
    /// Asserts that the return value is zero
    /// </summary>
    [TestMethod, TestCategory("CoinNative")]
        public void SonnetCoinNativeTest1()
        {
            Console.WriteLine("SonnetCoinNativeTest1: GamsTest");
            Assert.IsTrue(NativeTests.RunGamsTest() == 0);
        }

        /// <summary>
        /// Runs the Cbc native OsiUnitTest with SampleDir as mpsDir
        /// Asserts that the return value is zero
        /// </summary>
        [TestMethod, TestCategory("CoinNative")]
        public void SonnetCoinNativeTest2()
        {
            Console.WriteLine("SonnetCoinNativeTest2: Cbc's osiUnitTest.exe -mpsDir=" + SampleDir);
            Assert.IsTrue(System.IO.Directory.Exists(SampleDir), "SampleDir not found at " + SampleDir);
            Assert.IsTrue(NativeTests.RunOsiCbcUnitTest(SampleDir) == 0);
        }

        /// <summary>
        /// Runs the Cbc native cbc.exe unitTest with MipLibDir
        /// Asserts that the return value is zero
        /// </summary>
        [TestMethod, TestCategory("CoinNative")]
        public void SonnetCoinNativeTest3()
        {
#if DEBUG
            // in Debug, skip the miplib tests, since AIR03 takes far too long in DEBUG.
            Console.WriteLine("SonnetCoinNativeTest3 (DBG): cbc.exe -dirSample " + SampleDir + " -unitTest");
            Assert.IsTrue(System.IO.Directory.Exists(SampleDir), "SampleDir not found at " + SampleDir);
            Assert.IsTrue(NativeTests.RunCbc(SampleDir, null) == 0);
#else
            Console.WriteLine("SonnetCoinNativeTest3: cbc.exe -dirSample " + SampleDir + " -dirMiplib " + MipLibDir + " -unitTest");
            Assert.IsTrue(System.IO.Directory.Exists(SampleDir), "SampleDir not found at " + SampleDir);
            Assert.IsTrue(System.IO.Directory.Exists(MipLibDir), "MipLibDir not found at " + MipLibDir);
            Assert.IsTrue(NativeTests.RunCbc(SampleDir, MipLibDir) == 0);
#endif
        }

        /// <summary>
        /// Runs the Clp native OsiUnitTest with SampleDir as mpsDir
        /// Asserts that the return value is zero
        /// </summary>
        [TestMethod, TestCategory("CoinNative")]
        public void SonnetCoinNativeTest4()
        {
            Console.WriteLine("SonnetCoinNativeTest4: Clp's osiUnitTest.exe -mpsDir=" + SampleDir);
            Assert.IsTrue(System.IO.Directory.Exists(SampleDir), "SampleDir not found at " + SampleDir);
            Assert.IsTrue(NativeTests.RunOsiClpUnitTest(SampleDir) == 0);
        }

        // Skipping CInterfaceTest
    }
}