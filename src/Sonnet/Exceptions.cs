// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Text;

namespace Sonnet
{
    /// <summary>
    /// The SonnetException is thrown whenever a Sonnet-specific error occurs. It is derived from ApplicationException.
    /// </summary>
    public class SonnetException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the Sonnet.SonnetException class.
        /// </summary>
        public SonnetException()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the Sonnet.SonnetException class with
        /// a specified error message.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        public SonnetException(string message)
            : base (message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Sonnet.SonnetException class with
        /// a specified error message and a reference to the inner exception that is
        /// the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the innerException
        /// parameter is not a null reference, the current exception is raised in a catch
        /// block that handles the inner exception.</param>
        public SonnetException(string message, Exception innerException)
            :base(message, innerException)
        {
        }
    }
}
