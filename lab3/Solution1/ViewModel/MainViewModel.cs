﻿using ClassLibrary1;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Diagnostics;
using System.Windows.Input;
using System.Xml.Linq;

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

        private bool FileLoaded = false;
        private bool is_running = false;
        public string Text { get; set; }
        private readonly IUIServices uiServices;
        public ObservableCollection<Data> Messages { get; set; }
        public ICommand SendCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public string Question { get; set; }

        public TextAnalyzer Analyzer { get; set; }
        public async Task Send(object arg)
        {
            cts = new CancellationTokenSource();
            try
            {
                string quest = this.Question;
                Question = String.Empty;
                RaisePropertyChanged(nameof(Question));
                is_running = true;
                if (quest != null && quest != String.Empty)
                {
                    if (quest == "/load")
                    {
                        Messages.Add(new Data("/load", "Right"));
                        string? fileName = uiServices.OpenFile();
                        if (fileName != null)
                        {
                            Analyzer = await TextAnalyzer.CreateAsync(fileName, cts.Token);
                            Text = fileName;
                            if (System.IO.File.Exists("bert-large-uncased-whole-word-masking-finetuned-squad.onnx"))
                            {
                                FileLoaded = true;
                            }
                            Messages.Add(new Data(await File.ReadAllTextAsync(fileName, cts.Token), "Left"));
                        }
                        else
                        {
                            Messages.Add(new Data("Файл не выбран, напишите команду /load для выбора файла", "Left"));
                        }
                    }
                    else
                    {
                        Messages.Add(new Data(quest, "Right"));
                        if (FileLoaded == false)
                        {
                            string? fileName = uiServices.OpenFile();
                            if (fileName != null)
                            {
                                Analyzer = await TextAnalyzer.CreateAsync(fileName, cts.Token);
                                Text = fileName;
                                if (System.IO.File.Exists("bert-large-uncased-whole-word-masking-finetuned-squad.onnx"))
                                {
                                    FileLoaded = true;
                                }

                                Messages.Add(new Data(await File.ReadAllTextAsync(fileName, cts.Token), "Left"));
                            }
                            else
                            {
                                Messages.Add(new Data("Файл не выбран, напишите команду /load для выбора файла", "Left"));
                            }
                        }
                        if (FileLoaded == true)
                        {
                            string answer = await Base.BD_request(Text, quest, Analyzer, cts.Token);
                            Messages.Add(new Data(answer, "Left"));
                        }
                    }
                    
                }
            }
            catch(Exception ex)
            {
                uiServices.ReportError(ex.Message);
            }
            finally
            {
                is_running = false;
            }
        }

        public void Cancel(object arg)
        {
            cts.Cancel(); 
        }
        public void Delete(object arg)
        {
            Base.DeleteAll();
        }
        public MainViewModel(IUIServices uiServices)
        {
            FileLoaded = false;
            SendCommand = new AsyncRelayCommand(Send);
            CancelCommand = new RelayCommand(Cancel, _ => is_running);
            DeleteCommand = new RelayCommand(Delete, _ => !is_running);
            this.uiServices = uiServices;
            List<(string, string)> history = Base.ReadAll();
            Messages = new();
            foreach (var elem in history)
            {
                Messages.Add(new Data(elem.Item1, "Right"));
                Messages.Add(new Data(elem.Item2, "Left"));
            }
            RaisePropertyChanged(nameof(Messages));
        }
    }
}

