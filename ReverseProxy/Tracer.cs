using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ReverseProxy
{
    public interface ITracer
    {
        Task TraceCallAsync(string uri, bool succedeed);
    }

    public class LogFileTracer : ITracer
    {
        private string _traceFile = "tracing.txt";

        public async Task TraceCallAsync(string uri, bool succedeed)
        {
            //for performance I should hold a buffer and write asynchronously in a multi-threads way not for every call
            //for sake of  semplicity I write immediately

            Task.Run(() => {
                string message = $"{DateTime.UtcNow.ToString()},{uri},{succedeed}\n";
                Trace.WriteLine(message);
                File.AppendAllText(_traceFile,message);
            });

        }
    }
}
