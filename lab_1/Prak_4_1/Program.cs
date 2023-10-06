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
                    string question_copy = question;
                    var task1 = textAnalyzer.GetAnswerAsync(question_copy, cts.Token);
                    var task2 = task1.ContinueWith(t =>
                    {
                        var rez = task1.Result;
                        Console.WriteLine($"question: {question_copy}, answer: {rez}");
                    }, cts.Token);
                    tasks.Add(task1);
                   
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
