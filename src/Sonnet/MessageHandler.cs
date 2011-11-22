// Copyright (C) 2011, Jan-Willem Goossens 
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
    /// Singleton with standard MessaHandler (write to Console)
    /// </summary>
    public class SonnetLog
    {
        protected SonnetLog()
        {
            messageHandler = new MessageHandler();
            messages = new SonnetMessages();
        }

        private static SonnetLog _default = null;
        public static SonnetLog Default
        {
            get 
            {
                if (_default == null) _default = new SonnetLog();

                return _default;
            }
        }

        public void PassToSolver(COIN.OsiSolverInterface solver)
        {
            solver.passInMessageHandler(messageHandler);
        }

        public void PassToCoinMpsIO(COIN.CoinMpsIO obj)
        {
            obj.passInMessageHandler(messageHandler);
        }

        public int LogLevel
        {
            get { return messageHandler.logLevel(); }
            set { messageHandler.setLogLevel(value); }
        }

        public string Debug(string message)
        {
            messageHandler.message(SonnetMessages.Debug, messages, message);
            return message;
        }

        public string DebugFormat(string format, params object[] args)
        {
            return Debug(string.Format(format, args));
        }

        public string Error(string message)
        {
            messageHandler.message(SonnetMessages.Error, messages, message);
            return message;
        }

        public string ErrorFormat(string format, params object[] args)
        {
            return Error(string.Format(format, args));
        }

        public string Warn(string message)
        {
            messageHandler.message(SonnetMessages.Warning, messages, message);
            return message;
        }

        public string WarnFormat(string format, params object[] args)
        {
            return Warn(string.Format(format, args));
        }

        public string Info(string message)
        {
            messageHandler.message(SonnetMessages.Information, messages, message);
            return message;
        }

        public string InfoFormat(string format, params object[] args)
        {
            return Info(string.Format(format, args));
        }
        
        private COIN.CoinMessageHandler messageHandler;
        private COIN.CoinMessages messages;
    }
}
