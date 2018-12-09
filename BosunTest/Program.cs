using BosunReporter;
using BosunReporter.Metrics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;

namespace BosunTest
{
    class Program
    {
        static HttpClient _client = new HttpClient();
        static MetricsCollector _collector;
        static EventGauge _timer;

        static void Main(string[] args)
        {

            _collector = new MetricsCollector(new BosunOptions(ex => Handle(ex))
            {
                MetricsNamePrefix = "TestApp.",
                BosunUrl = new System.Uri("http://192.168.1.5:8070"),
                PropertyToTagName = NameTransformers.CamelToLowerSnakeCase,
                ThrowOnPostFail = true,
                DefaultTags = new Dictionary<string, string>
        { {"host", NameTransformers.Sanitize(Environment.MachineName.ToLower())} }
            });

            _timer = _collector.CreateMetric<EventGauge>("GetWeather", "time taken", "measures time taken to get weather from api");

            var dispatcherTimer = new Timer();
            dispatcherTimer.Interval = 5000;
            dispatcherTimer.Start();
            dispatcherTimer.Elapsed += new ElapsedEventHandler(timer_Elapsed);

            Console.ReadLine();
        }



        static async void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Time(async () =>
            {
                await GetWeather();
            }, _timer);

        }

        static async Task GetWeather()
        {

            HttpResponseMessage response = await _client.GetAsync("https://samples.openweathermap.org/data/2.5/weather?q=London,uk&appid=b6907d289e10d714a6e88b30761fae22");

            Console.WriteLine("Got Weather");
        }

        public static async Task Time(Func<Task> action, EventGauge gauge)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            await action.Invoke();
            stopwatch.Stop();
            gauge.Record(stopwatch.Elapsed.TotalMilliseconds, DateTime.Now);


        }

        static void Handle(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
