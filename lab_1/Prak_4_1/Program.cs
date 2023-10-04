using BERTTokenizers;
using Microsoft.ML.Data;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using ClassLibrary1;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        public static CancellationTokenSource cts = new CancellationTokenSource();

        protected static void myHandler(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            cts.Cancel();
        }
        static async Task Main(string[] args)
        {
            try
            {
                Console.CancelKeyPress += new ConsoleCancelEventHandler(myHandler);
                IProgress<double> p = new Progress<double>(progress =>
                {
                    Console.WriteLine("Downloaded: {0:f3} Mbyte", progress);
                });


                TextAnalyzer textAnalyzer = await TextAnalyzer.CreateAsync(args.FirstOrDefault(), cts.Token, p);
                string question;

                var tasks = new List<Task>();
                while ((question = Console.ReadLine() ?? string.Empty) != string.Empty)
                {
                    var task1 = textAnalyzer.GetAnswerAsync(question, cts.Token);
                    var task2 = task1.ContinueWith(t =>
                    {
                        var rez = t.Result;
                        Console.WriteLine($"question: {rez.Item1}, answer: {rez.Item2}");
                    }, cts.Token);
                    tasks.Add(task2);
                }
                Task.WaitAll(tasks.ToArray());

            }

            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
        }
    }
}
