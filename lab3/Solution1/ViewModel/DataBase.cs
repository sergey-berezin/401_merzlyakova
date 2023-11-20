using ClassLibrary1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace ViewModel
{
    public class DataBase
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }
    public class Base
    {
        private const string file_json = "data.json";
        private const string file_copy_json = "data_copy.json";
        public static async Task<string> BD_request(string question, TextAnalyzer analyzer, CancellationToken token)
        {
            DataBase newData;
            List <DataBase > all_data = null;
            using (FileStream fs = new FileStream(file_json, FileMode.OpenOrCreate))
            {
                DataBase[]? tmp = fs.Length > 0 ? await JsonSerializer.DeserializeAsync<DataBase[]?>(fs) : null;
                if (tmp != null)
                {
                    all_data = new List<DataBase>(tmp);
                }
                else
                {
                    all_data = new List<DataBase>();
                }
                foreach (var data in all_data)
                {
                    if (data.Question == question)
                    {
                        return data.Answer;
                    }
                }
                newData = new DataBase { Question = question, Answer = await analyzer.GetAnswerAsync(question, token) };
                all_data.Add(newData);
            }
            try
            {
                System.IO.File.Move(file_json, file_copy_json);
                using (FileStream fs = new FileStream(file_json, FileMode.CreateNew))
                {
                    await JsonSerializer.SerializeAsync<DataBase[]>(fs, all_data.ToArray());
                }
                System.IO.File.Delete(file_copy_json);
            }
            catch (Exception ex) {
                System.IO.File.Delete(file_json);
                System.IO.File.Move(file_copy_json, file_json);
            }
            return newData.Answer;
        }
        public static void DeleteAll()
        {
            File.WriteAllText(file_json, string.Empty);
        }
        public static List<(string, string)> ReadAll()
        {
            using (FileStream fs = new FileStream(file_json, FileMode.OpenOrCreate))
            {
                DataBase[]? all_data = fs.Length > 0 ? JsonSerializer.Deserialize<DataBase[]?>(fs) : null;
                var res = new List<(string, string)>();
                if (all_data != null)
                {
                    foreach (var data in all_data)
                    {
                        res.Add((data.Question, data.Answer));
                    }
                }
                return res;
            }
        }
    }
}
