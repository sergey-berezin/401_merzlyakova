using ClassLibrary1;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ViewModel
{
    public interface IUIServices
    {
        void ReportError(string message);
        string? SaveFile();
        string? OpenFile();

    }
    public class Data
    {
        public string Text { get; set; }
        public string Alignment { get; set; }
        public Data(string text, string alignment)
        {
            Text = text;
            Alignment = alignment;
        }
    }
    

    public class MainViewModel : ViewModelBase
    {
        public CancellationTokenSource cts { get; set; }
        private bool FileLoaded { get; set; }
        private bool is_detecting = false;
        public string Text { get; set; }
        private readonly IUIServices uiServices;
        public ObservableCollection<Data> Messages { get; set; }
        public ICommand SendCommand { get; private set; }
        public string Question { get; set; }
        public async Task Send(object arg)
        {
            if (Question != null && Question != String.Empty)
            {
                if (Question == "/load")
                {
                    Messages.Add(new Data("/load", "Right"));
                    string? fileName = uiServices.OpenFile();
                    if (fileName != null)
                    {
                        //создать объект с текстом
                        FileLoaded = true;
                        //можно как то оповещать об ожиданиие(флаг)
                        Messages.Add(new Data(await File.ReadAllTextAsync(fileName, cts.Token), "Left"));
                    }
                    else
                    {
                        Messages.Add(new Data("Файл не выбран, напишите команду /load для выбора файла", "Left"));
                    }
                }
                else
                {
                    Messages.Add(new Data(Question, "Right"));
                    if (FileLoaded == false)
                    {
                        string? fileName = uiServices.OpenFile();
                        if (fileName != null)
                        {
                            //создать объект с текстом
                            FileLoaded = true;
                        }
                        else
                        {
                            Messages.Add(new Data("Файл не выбран, напишите команду /load для выбора файла", "Left"));
                        }
                    }
                    if (FileLoaded == true)
                    {

                        Messages.Add(new Data("ХЗХЗХЗХЗ", "Left"));
                    }

                    // ans = model.get_answer(Question)

                }
            }



            //Messages.Add(new Data("asd", "Right"));
            //Messages.Add(new Data("asdas", "Left"));
            //RaisePropertyChanged(nameof(Text));
        }



        public MainViewModel(IUIServices uiServices)
        {
            cts = new();
            FileLoaded = false;
            SendCommand = new AsyncRelayCommand(Send);
            this.uiServices = uiServices;
            Messages = new();
        }
    }
}

        