﻿using LionFire.Structures;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TLogger = Microsoft.Extensions.Logging.ILogger;
using TLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace LionFire.Logging.Null
{
    public class NullLogger : NullLogger<object>
    {

    }
    public class NullLogger<T> : TLogger, ILogger<T>
    {
        public static NullLogger Instance { get { return Singleton<NullLogger>.Instance;  } }
        public IDisposable BeginScope<TState>(TState state)
        {
            return Singleton<NoopDisposable>.Instance;
        }

        public bool IsEnabled(TLogLevel logLevel)
        {
            return false;
        }

        public void Log<TState>(TLogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
        }
        
    }
}
