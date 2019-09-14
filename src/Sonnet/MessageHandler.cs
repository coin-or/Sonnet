// Copyright (C) Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

using System;
using System.Collections.Generic;
using System.Text;

namespace Sonnet
{
    internal class MessageHandler : COIN.CoinMessageHandler
    {
        public MessageHandler()
        {
        }

        public override void setLogLevel(int value)
        {
            base.setLogLevel(value);
        }

        public override int print()
        {
            string message = messageBuffer();
            Console.WriteLine(message);

            return 0;
        }
    }

    /// <summary>
    /// The class SonnetMessages holds the specifications for the different types of messages.
    /// The message types are matched to COIN message numbers, in particular 
    /// Debug (0), Information (1000), Warning (3000) and Error (6000).
    /// The Source is hardcoded to 'SNNT'.
    /// </summary>
    internal class SonnetMessages : COIN.CoinMessages
    {
        public const int Debug = 0;
        public const int Information = 1;
        public const int Warning = 2;
        public const int Error = 3;

        public SonnetMessages()
            : base(2)
        {
            source = "SNNT";
            addMessage(Debug, new COIN.CoinOneMessage(0, 3, "%s"));
            addMessage(Information, new COIN.CoinOneMessage(1000, 2, "%s"));
            addMessage(Warning, new COIN.CoinOneMessage(3000, 1, "%s"));
            addMessage(Error, new COIN.CoinOneMessage(6000, 1, "%s"));
        }
    }

    /// <summary>
    /// Singleton with standard MessaHandler (write to Console) and use the defined SonnetMessages types
    /// Debug, Information, Warning and Error.
    /// </summary>
    public class SonnetLog
    {
        /// <summary>
        /// Protected constructor 
        /// </summary>
        protected SonnetLog()
        {
            messageHandler = new MessageHandler();
            messages = new SonnetMessages();
        }

        private static SonnetLog _default = null;
        /// <summary>
        /// Returns the singleton SonnetLog
        /// </summary>
        public static SonnetLog Default
        {
            get 
            {
                if (_default == null) _default = new SonnetLog();

                return _default;
            }
        }

        /// <summary>
        /// Pass this message handler to the given solver.
        /// </summary>
        /// <param name="solver">The solver to pass this message handler to.</param>
        public void PassToSolver(COIN.OsiSolverInterface solver)
        {
            solver.passInMessageHandler(messageHandler);
        }

        /// <summary>
        /// Pass this message handler to the given CoinMpsIO instance.
        /// </summary>
        /// <param name="obj">The CoinMpsIO instance to pass this message handler to.</param>
        public void PassToCoinMpsIO(COIN.CoinMpsIO obj)
        {
            obj.passInMessageHandler(messageHandler);
        }

        /// <summary>
        /// Pass this message handler to the given CoinLpIO instance.
        /// </summary>
        /// <param name="obj">The CoinLpIO instance to pass this message handler to.</param>
        public void PassToCoinLpIO(COIN.CoinLpIO obj)
        {
            obj.passInMessageHandler(messageHandler);
        }

        /// <summary>
        /// Gets or sets the logLevel of this message handler. See COIN documentation.
        /// </summary>
        public int LogLevel
        {
            get { return messageHandler.logLevel(); }
            set { messageHandler.setLogLevel(value); }
        }

        /// <summary>
        /// Write a Debug message (log level 0) to this message handler.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <returns>The logged message.</returns>
        public string Debug(string message)
        {
            messageHandler.message(SonnetMessages.Debug, messages, message);
            return message;
        }

        /// <summary>
        /// Write a formatted Debug message (log level 0) to this message handler.
        /// See string.Format(..) : 
        /// Replaces the format item in a specified string with the string representation
        ///  of a corresponding object in a specified array.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>The logged message.</returns>
        public string DebugFormat(string format, params object[] args)
        {
            return Debug(string.Format(format, args));
        }

        /// <summary>
        /// Write an Information message (log level 1000) to this message handler.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <returns>The logged message.</returns>
        public string Info(string message)
        {
            messageHandler.message(SonnetMessages.Information, messages, message);
            return message;
        }

        /// <summary>
        /// Write a formatted Information message (log level 1000) to this message handler.
        /// See string.Format(..) : 
        /// Replaces the format item in a specified string with the string representation
        ///  of a corresponding object in a specified array.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>The logged message.</returns>
        public string InfoFormat(string format, params object[] args)
        {
            return Info(string.Format(format, args));
        }

        /// <summary>
        /// Write a Warning message (log level 3000) to this message handler.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <returns>The logged message.</returns>
        public string Warn(string message)
        {
            messageHandler.message(SonnetMessages.Warning, messages, message);
            return message;
        }

        /// <summary>
        /// Write a formatted Warning message (log level 3000) to this message handler.
        /// See string.Format(..) : 
        /// Replaces the format item in a specified string with the string representation
        ///  of a corresponding object in a specified array.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>The logged message.</returns>
        public string WarnFormat(string format, params object[] args)
        {
            return Warn(string.Format(format, args));
        }

        /// <summary>
        /// Write an Error message (log level 6000) to this message handler.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <returns>The logged message.</returns>
        public string Error(string message)
        {
            messageHandler.message(SonnetMessages.Error, messages, message);
            return message;
        }

        /// <summary>
        /// Write a formatted Error message (log level 6000) to this message handler.
        /// See string.Format(..) : 
        /// Replaces the format item in a specified string with the string representation
        ///  of a corresponding object in a specified array.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>The logged message.</returns>
        public string ErrorFormat(string format, params object[] args)
        {
            return Error(string.Format(format, args));
        }

        private COIN.CoinMessageHandler messageHandler;
        private COIN.CoinMessages messages;
    }
}
