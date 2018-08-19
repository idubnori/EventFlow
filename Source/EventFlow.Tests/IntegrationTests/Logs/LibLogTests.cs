// The MIT License (MIT)
// 
// Copyright (c) 2015-2018 Rasmus Mikkelsen
// Copyright (c) 2015-2018 eBay Software Foundation
// https://github.com/eventflow/EventFlow
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using EventFlow.Extensions;
using EventFlow.TestHelpers;
using FluentAssertions;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using NLog.Targets;
using NUnit.Framework;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using ILog = EventFlow.Logs.ILog;
using LogLevel = EventFlow.Logs.LogLevel;

namespace EventFlow.Tests.IntegrationTests.Logs
{
    public class LibLogTests : Test
    {
        [Test]
        public void NlogTest()
        {
            var memoryTarget = new MemoryTarget
            {
                Layout = "${level:uppercase=true}|${message}"
            };
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(memoryTarget, NLog.LogLevel.Trace);

            using (var resolver = EventFlowOptions.New
                .UseLibLog(LibLogProviders.NLog)
                .CreateResolver())
            {
                var log = resolver.Resolve<ILog>();
                void TestLog(LogLevel logLevel, NLog.LogLevel level)
                {
                    var message = Guid.NewGuid().ToString("N");
                    log.Write(logLevel, message);
                    memoryTarget.Logs.Any(l => l.Contains(level.ToString().ToUpper()) && l.Contains(message)).Should().BeTrue();
                }

                TestLog(LogLevel.Verbose, NLog.LogLevel.Trace);
                TestLog(LogLevel.Debug, NLog.LogLevel.Debug);
                TestLog(LogLevel.Information, NLog.LogLevel.Info);
                TestLog(LogLevel.Warning, NLog.LogLevel.Warn);
                TestLog(LogLevel.Error, NLog.LogLevel.Error);
                TestLog(LogLevel.Fatal, NLog.LogLevel.Fatal);
            }
        }

        [Test]
        public void Log4NetTest()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var memoryAppender = new MemoryAppender();
            memoryAppender.ActivateOptions();
            hierarchy.Root.AddAppender(memoryAppender);
            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;

            using (var resolver = EventFlowOptions.New
                .UseLibLog(LibLogProviders.Log4Net)
                .CreateResolver())
            {
                var log = resolver.Resolve<ILog>();
                void TestLog(LogLevel logLevel, Level level)
                {
                    var message = Guid.NewGuid().ToString("N");
                    log.Write(logLevel, message);
                    memoryAppender.GetEvents().Any(e => e.Level == level && e.RenderedMessage == message).Should().BeTrue();
                }

                TestLog(LogLevel.Verbose, Level.Debug);
                TestLog(LogLevel.Debug, Level.Debug);
                TestLog(LogLevel.Information, Level.Info);
                TestLog(LogLevel.Warning, Level.Warn);
                TestLog(LogLevel.Error, Level.Error);
                TestLog(LogLevel.Fatal, Level.Fatal);
            }
        }

        [Test]
        public void SerilogTest()
        {
            var messages = new Dictionary<LogEventLevel, List<string>>
            {
                {LogEventLevel.Verbose, new List<string>()},
                {LogEventLevel.Debug, new List<string>()},
                {LogEventLevel.Information, new List<string>()},
                {LogEventLevel.Warning, new List<string>()},
                {LogEventLevel.Error, new List<string>()},
                {LogEventLevel.Fatal, new List<string>()}
            };

            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Sink(new DummySink(messages))
                .CreateLogger();

            using (var resolver = EventFlowOptions.New
                .UseLibLog(LibLogProviders.Serilog)
                .CreateResolver())
            {
                var log = resolver.Resolve<ILog>();
                void TestLog(LogLevel logLevel, LogEventLevel logEventLevel)
                {
                    var message = Guid.NewGuid().ToString("N");
                    log.Write(logLevel, message);
                    messages[logEventLevel].FirstOrDefault(m => m == message).Should().NotBeNullOrEmpty();
                }

                TestLog(LogLevel.Verbose, LogEventLevel.Verbose);
                TestLog(LogLevel.Debug, LogEventLevel.Debug);
                TestLog(LogLevel.Information, LogEventLevel.Information);
                TestLog(LogLevel.Warning, LogEventLevel.Warning);
                TestLog(LogLevel.Error, LogEventLevel.Error);
                TestLog(LogLevel.Fatal, LogEventLevel.Fatal);
            }
        }

        private class DummySink : ILogEventSink
        {
            private readonly Dictionary<LogEventLevel, List<string>> _messages;

            public DummySink(Dictionary<LogEventLevel, List<string>> messages)
            {
                _messages = messages;
            }

            public void Emit(LogEvent logEvent)
            {
                _messages[logEvent.Level].Add(logEvent.RenderMessage());
            }
        }
    }
}
