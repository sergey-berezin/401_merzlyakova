using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using BERTTokenizers;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Polly;

namespace ClassLibrary1
{
    public class TextAnalyzer
    {
        public static async Task<TextAnalyzer> CreateAsync(string path_text, CancellationToken token, IProgress<double>? downloadProgress = null)
        {
            if (!System.IO.File.Exists("bert-large-uncased-whole-word-masking-finetuned-squad.onnx"))
            {
                try
                {
                    using (FileStream fileStream = new FileStream("bert-large-uncased-whole-word-masking-finetuned-squad.onnx", FileMode.Create))
                    {
                        var jitterer = new Random();
                        var retryPolicy = Policy
                            .Handle<HttpRequestException>()
                            .WaitAndRetryAsync(5,
                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                          + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)));

                        HttpClient httpClient = new HttpClient();
                        var stream = await retryPolicy.ExecuteAsync(async () =>
                        {
                            Console.WriteLine("Getting data...");
                            return await httpClient.GetStreamAsync("https://storage.yandexcloud.net/dotnet4/bert-large-uncased-whole-word-masking-finetuned-squad.onnx", token);
                        });
                        byte[] buffer = new byte[1024];
                        double bytesReadTotal = 0;
                        int BytesRead = 0;
                        while ((BytesRead = await stream.ReadAsync(buffer, 0, 1024, token)) != 0)
                        {
                            bytesReadTotal += (double)BytesRead / (1024 * 1024);
                            await fileStream.WriteAsync(buffer, 0, BytesRead, token);
                            if (downloadProgress != null)
                            {
                                downloadProgress.Report(bytesReadTotal);
                            }

                        }
                    }
                } 
                catch (Exception ex) 
                {
                    if (System.IO.File.Exists("bert-large-uncased-whole-word-masking-finetuned-squad.onnx"))
                    {
                        File.Delete("bert-large-uncased-whole-word-masking-finetuned-squad.onnx");
                    }
                    Console.WriteLine(ex.Message);
                }

            }
            try
            {
                var textAnalyzer = new TextAnalyzer(path_text);
                textAnalyzer.text = await textAnalyzer.streamreader.ReadToEndAsync();
                return textAnalyzer;

            }

            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return new TextAnalyzer(path_text);///////////////////
            }
            
        }


        StreamReader streamreader;
        string text;
        public TextAnalyzer(string path_text) 
        {
            streamreader = new StreamReader(path_text);
            text = string.Empty;
        }
        public async Task Run()
        {
            List<Tuple<string, string>> ans = new List<Tuple<string, string>>();
            string question;
            await Task.Factory.StartNew(() =>
            {
                MakeTask(Console.ReadLine() ?? string.Empty);
            }, TaskCreationOptions.LongRunning);
        }

        private void MakeTask(string question)
        {
            if (question != string.Empty)
            {
                var task1 = Task.Factory.StartNew(() =>
                {
                    MakeTask(Console.ReadLine() ?? string.Empty);
                }, TaskCreationOptions.LongRunning);
                var task2 = Task<Tuple<string, string>>.Factory.StartNew(() =>
                {
                    return GetAnswer(question);
                }, TaskCreationOptions.LongRunning);
                Console.WriteLine($"question: {task2.Result.Item1}, answer: {task2.Result.Item2}");
                task1.Wait();
            }
        }
        private Tuple<string, string> GetAnswer(string question)
        {
            var sentence = $"\"question\": \"{question}\" \"context\": \"{text}\"";
            var tokenizer = new BertUncasedLargeTokenizer();
            var tokens = tokenizer.Tokenize(sentence);
            var encoded = tokenizer.Encode(tokens.Count(), sentence);
            var bertInput = new BertInput()
            {
                InputIds = encoded.Select(t => t.InputIds).ToArray(),
                AttentionMask = encoded.Select(t => t.AttentionMask).ToArray(),
                TypeIds = encoded.Select(t => t.TokenTypeIds).ToArray(),
            };
            var modelPath = "bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
            var input_ids = ConvertToTensor(bertInput.InputIds, bertInput.InputIds.Length);
            var attention_mask = ConvertToTensor(bertInput.AttentionMask, bertInput.InputIds.Length);
            var token_type_ids = ConvertToTensor(bertInput.TypeIds, bertInput.InputIds.Length);
            var input = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input_ids", input_ids),
                                                    NamedOnnxValue.CreateFromTensor("input_mask", attention_mask),
                                                    NamedOnnxValue.CreateFromTensor("segment_ids", token_type_ids) };
            var session = new InferenceSession(modelPath);
            var output = session.Run(input);
            List<float> startLogits = (output.ToList().First().Value as IEnumerable<float>).ToList();
            List<float> endLogits = (output.ToList().Last().Value as IEnumerable<float>).ToList();
            var startIndex = startLogits.ToList().IndexOf(startLogits.Max());
            var endIndex = endLogits.ToList().IndexOf(endLogits.Max());
            var predictedTokens = tokens
                        .Skip(startIndex)
                        .Take(endIndex + 1 - startIndex)
                        .Select(o => tokenizer.IdToToken((int)o.VocabularyIndex))
                        .ToList();
            return new (question, String.Join(" ", predictedTokens));
        }
        private static Tensor<long> ConvertToTensor(long[] inputArray, int inputDimension)
        {
            // Create a tensor with the shape the model is expecting. Here we are sending in 1 batch with the inputDimension as the amount of tokens.
            Tensor<long> input = new DenseTensor<long>(new[] { 1, inputDimension });

            // Loop through the inputArray (InputIds, AttentionMask and TypeIds)
            for (var i = 0; i < inputArray.Length; i++)
            {
                // Add each to the input Tenor result.
                // Set index and array value of each input Tensor.
                input[0, i] = inputArray[i];
            }
            return input;
        }
        private class BertInput
        {
            public long[] InputIds { get; set; }
            public long[] AttentionMask { get; set; }
            public long[] TypeIds { get; set; }
        }
    }
}
