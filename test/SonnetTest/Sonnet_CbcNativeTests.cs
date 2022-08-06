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
    public class Sonnet_CbcNativeTests
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
        /// Run native Cbc tests. 
        /// Set SampleDir directly, or via environment variable SONNET_SAMPLEDIR.
        /// Same for MipLibDir via SONNET_MIPLIBDIR
        /// </summary>
        public Sonnet_CbcNativeTests()
        {
            SampleDir = Environment.GetEnvironmentVariable("SONNET_SAMPLEDIR");
            MipLibDir = Environment.GetEnvironmentVariable("SONNET_MIPLIBDIR");

#if NETCOREAPP
            // By default, the executable will be one directory deeper in NET5 compared to NET4
            SampleDir = SampleDir ?? "..\\..\\..\\..\\..\\..\\..\\..\\Data-sample";
            MipLibDir = MipLibDir ?? "..\\..\\..\\..\\..\\..\\..\\..\\Data-miplib3";
#else
            SampleDir = SampleDir ?? "..\\..\\..\\..\\..\\..\\..\\Data-sample";
            MipLibDir = MipLibDir ?? "..\\..\\..\\..\\..\\..\\..\\Data-miplib3";
#endif

    }
    /// <summary>
    /// Runs the Cbc native gams test.
    /// Asserts that the return value is zero
    /// </summary>
    [TestMethod, TestCategory("CbcNative")]
        public void SonnetCbcNativeTest1()
        {
            Console.WriteLine("SonnetCbcNativeTest1: GamsTest");

            Assert.IsTrue(CbcNativeTests.RunGamsTest() == 0);
        }

        /// <summary>
        /// Runs the Cbc native OsiUnitTest with SampleDir as mpsDir
        /// Asserts that the return value is zero
        /// </summary>
        [TestMethod, TestCategory("CbcNative")]
        public void SonnetCbcNativeTest2()
        {
            Console.WriteLine("SonnetCbcNativeTest2: osiUnitTest.exe -mpsDir=" + SampleDir);

            Assert.IsTrue(System.IO.Directory.Exists(SampleDir), "SampleDir not found at " + SampleDir);
            Assert.IsTrue(CbcNativeTests.RunOsiUnitTest(SampleDir) == 0);
        }

        /// <summary>
        /// Runs the Cbc native cbc.exe unitTest with MipLibDir
        /// Asserts that the return value is zero
        /// </summary>
        [TestMethod, TestCategory("CbcNative")]
        public void SonnetCbcNativeTest3()
        {
#if DEBUG
            // in Debug, skip the miplib tests, since AIR03 takes far too long in DEBUG.
            Console.WriteLine("SonnetCbcNativeTest3 (DBG): cbc.exe -dirSample " + SampleDir + " -unitTest");
            Assert.IsTrue(System.IO.Directory.Exists(SampleDir), "SampleDir not found at " + SampleDir);
            Assert.IsTrue(CbcNativeTests.RunCbc(SampleDir, null) == 0);
#else
            Console.WriteLine("SonnetCbcNativeTest3: cbc.exe -dirSample " + SampleDir + " -dirMiplib " + MipLibDir + " -unitTest");
            Assert.IsTrue(System.IO.Directory.Exists(SampleDir), "SampleDir not found at " + SampleDir);
            Assert.IsTrue(System.IO.Directory.Exists(MipLibDir), "MipLibDir not found at " + MipLibDir);
            Assert.IsTrue(CbcNativeTests.RunCbc(SampleDir, MipLibDir) == 0);
#endif
        }

        // Skipping CInterfaceTest
    }
}