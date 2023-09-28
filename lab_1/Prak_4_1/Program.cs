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
        static async Task Main(string[] args)
        {
            IProgress<double> p = new Progress<double>(progress =>
            {
                Console.WriteLine("Downloaded: {0:f3} Mbyte", progress);
            });

            CancellationTokenSource cts = new CancellationTokenSource();
            TextAnalyzer textAnalyzer = await TextAnalyzer.CreateAsync(args.FirstOrDefault(), cts.Token, p);
            await textAnalyzer.Run();



        }
    }
}
