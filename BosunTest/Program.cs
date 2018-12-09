using BosunReporter;
using BosunReporter.Metrics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace BosunTest
{
    class Program
    {
        static HttpClient client = new HttpClient();
        static MetricsCollector collector = new MetricsCollector(new BosunOptions(ex => Handle(ex))
        {
            MetricsNamePrefix = "TestApp.",
            BosunUrl = new System.Uri("http://192.168.1.5:8070"),
            PropertyToTagName = NameTransformers.CamelToLowerSnakeCase,
            ThrowOnPostFail = true,
            DefaultTags = new Dictionary<string, string>
        { {"host", NameTransformers.Sanitize(Environment.MachineName.ToLower())} }
        });
        static EventGauge timer = collector.CreateMetric<EventGauge>("GetWeather", "time taken", "measures time taken to get weather from api");

        static void Main(string[] args)
        {

            RunAsync().GetAwaiter().GetResult();

            Console.ReadLine();
        }

        static async Task RunAsync()
        {

            await GetWeather();
        }

        static async Task GetWeather()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            HttpResponseMessage response = await client.GetAsync("https://samples.openweathermap.org/data/2.5/weather?q=London,uk&appid=b6907d289e10d714a6e88b30761fae22");
            stopwatch.Stop();
            timer.Record(stopwatch.Elapsed.TotalMilliseconds, DateTime.Now);
        }

        static void Handle(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
