// Copyright (C) Jan-Willem Goossens 
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonnetTest
{
    [TestClass]
    public class Sonnet_ExampleTests
    {
        [TestMethod, TestCategory("Example")]
        public void SonnetExampleTest1()
        {
            var ex = new SonnetExamples.Example1();
            ex.Run();
        }

        [TestMethod, TestCategory("Example")]
        public void SonnetExampleTest2()
        {
            var ex = new SonnetExamples.Example2();
            ex.Run();
        }

        [TestMethod, TestCategory("Example")]
        public void SonnetExampleTest3()
        {
            var ex = new SonnetExamples.Example3();
            ex.Run();
        }

        [TestMethod, TestCategory("Example")]
        public void SonnetExampleTest4()
        {
            var ex = new SonnetExamples.Example4.Example4();
            ex.Run();
        }

        [TestMethod, TestCategory("Example")]
        public void SonnetExampleTest5()
        {
            var ex = new SonnetExamples.Example5.Example5();
            ex.Run();
        }

        [TestMethod, TestCategory("Example")]
        public void SonnetExampleTest6()
        {
            var ex = new SonnetExamples.Example6.Example();
            ex.Run();
        }

        [TestMethod, TestCategory("Example")]
        public void SonnetExampleTest6b()
        {
            var ex = new SonnetExamples.Example6b.Example();
            ex.Run();
        }
    }
}
