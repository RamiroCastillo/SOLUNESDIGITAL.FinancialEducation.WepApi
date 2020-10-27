using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace SOLUNESDIGITAL.Framework.Logs
{
    public interface ILogger
    {
    }

    public class Logger : ILogger
    {
        private static string _pathLogFile;
        public const string header = "INFO: {0} DETALLE: {1}";

        public Logger(string pathLogFile, string level)
        {
            switch (level)
            {
                case "INFORMATION":
                    _pathLogFile = pathLogFile;
                    Log.Logger = new LoggerConfiguration()
                        .Enrich.With(new ThreadIdEnricher())
                        .WriteTo.File(_pathLogFile, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] <{ThreadId}> {Message:lj}{NewLine}{Exception}")
                        .MinimumLevel.Information()
                        .CreateLogger();
                    break;
                case "FATAL":
                    _pathLogFile = pathLogFile;
                    Log.Logger = new LoggerConfiguration()
                        .Enrich.With(new ThreadIdEnricher())
                        .WriteTo.File(_pathLogFile, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] <{ThreadId}> {Message:lj}{NewLine}{Exception}")
                        .MinimumLevel.Fatal()
                        .CreateLogger();
                    break;
                case "WARNING":
                    _pathLogFile = pathLogFile;
                    Log.Logger = new LoggerConfiguration()
                        .Enrich.With(new ThreadIdEnricher())
                        .WriteTo.File(_pathLogFile, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] <{ThreadId}> {Message:lj}{NewLine}{Exception}")
                        .MinimumLevel.Warning()
                        .CreateLogger();
                    break;
                case "ERROR":
                    _pathLogFile = pathLogFile;
                    Log.Logger = new LoggerConfiguration()
                        .Enrich.With(new ThreadIdEnricher())
                        .WriteTo.File(_pathLogFile, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] <{ThreadId}> {Message:lj}{NewLine}{Exception}")
                        .MinimumLevel.Error()
                        .CreateLogger();
                    break;
                case "DEBUG":
                    _pathLogFile = pathLogFile;
                    Log.Logger = new LoggerConfiguration()
                        .Enrich.With(new ThreadIdEnricher())
                        .WriteTo.File(_pathLogFile, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] <{ThreadId}> {Message:lj}{NewLine}{Exception}")
                        .MinimumLevel.Debug()
                        .CreateLogger();
                    break;
                default:
                    _pathLogFile = pathLogFile;
                    Log.Logger = new LoggerConfiguration()
                        .Enrich.With(new ThreadIdEnricher())
                        .WriteTo.File(_pathLogFile, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] <{ThreadId}> {Message:lj}{NewLine}{Exception}")
                        .MinimumLevel.Verbose()
                        .CreateLogger();
                    break;
            }
        }

        class ThreadIdEnricher : ILogEventEnricher
        {
            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                        "ThreadId", Thread.CurrentThread.ManagedThreadId));
            }
        }

        private static string GetStackTraceInfo()
        {
            var stackFrame = new StackTrace().GetFrame(2);
            string methodName = stackFrame.GetMethod().Name;
            string className = stackFrame.GetMethod().ReflectedType.FullName;
            return string.Format("Class: \"{0}\" Method: \"{1}\"", className, methodName);
        }

        public static void Debug(string format, params object[] objects)
        {
            string location = GetStackTraceInfo();
            string message = string.Format(format, objects);
            Log.Debug(string.Format(header, location, message));
        }

        public static void Debug(string message)
        {
            string location = GetStackTraceInfo();
            Log.Debug(string.Format(header, location, message));
        }

        public static void Error(string format, params object[] objects)
        {
            string message = string.Format(format, objects);
            string location = GetStackTraceInfo();
            Log.Error(string.Format(header, location, message));
        }

        public static void Error(string message)
        {
            string location = GetStackTraceInfo();
            Log.Error(string.Format(header, location, message));
        }

        public static void Fatal(string format, params object[] objects)
        {
            string message = string.Format(format, objects);
            string location = GetStackTraceInfo();
            Log.Fatal(string.Format(header, location, message));
        }

        public static void Fatal(string message)
        {
            string location = GetStackTraceInfo();
            Log.Fatal(string.Format(header, location, message));
        }
    }
}
