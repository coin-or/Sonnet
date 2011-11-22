// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Text;

namespace Sonnet
{
    public class SonnetException : ApplicationException
    {
        public SonnetException()
        {

        }

        public SonnetException(string message)
            : base (message)
        {
        }

        public SonnetException(string message, Exception innerException)
            :base(message, innerException)
        {
        }
    }
}
