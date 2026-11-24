using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PersistentEmpires.Views
{
    public class SentryForView
    {
        // public static RavenClient Raven;
        public static void InitializeSentry()
        {
            AppDomain.CurrentDomain.UnhandledException += HandleExceptionalExit;
        }

        public static void OnExceptionThrown(Exception e)
        {
            var message = $"Exception{Environment.NewLine}" +
                $"Exception: {e.ToLogString()}";

            try
            {
                var path = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\"));

                path += $"Error_{DateTime.Now.ToString("yyyyMMdd_hhmmss_fff")}.txt";
                using (FileStream fs = File.Create(path))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.Write(message);
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public static void RglExceptionThrown(StackTrace obj, Exception rglException)
        {

        }

        private static void HandleExceptionalExit(object sender, UnhandledExceptionEventArgs args)
        {
            var exception = args.ExceptionObject as Exception;
            var message = $"Exception{Environment.NewLine}" +
                $"Sender: {sender?.ToString()}{Environment.NewLine}" +
                $"Exception: {(exception != null ? exception.ToLogString() : args.ExceptionObject?.ToString())}";

            try
            {
                var path = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\"));

                path += $"Error_{DateTime.Now.ToString("yyyyMMdd_hhmmss_fff")}.txt";
                using (FileStream fs = File.Create(path))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.Write(message);
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public static void Dispose()
        {
        }
    }

    internal static class ExceptionExtensions
    {
        internal static string ToLogString(this Exception ex)
        {
            var message = $"Message: {ex.Message}{Environment.NewLine}" +
                $"Stack: {ex.StackTrace}{Environment.NewLine}" +
                $"InnerException: {ex.InnerException}{Environment.NewLine}" +
                $"Data: {ex.Data}{Environment.NewLine}" +
                $"Source: {ex.Source}";

            return message;
        }
    }
}