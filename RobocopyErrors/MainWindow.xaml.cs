using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RobocopyErrors
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Regex errorrx = new Regex(@"(?<date>\d\d\d\d\/\d\d\/\d\d\s\d\d:\d\d:\d\d)\s(?<error>ERROR\s\d+\s\(.*\))\s(?<operation>(.*))\s(?<path>\\\\yourservernamehere.*)", RegexOptions.Compiled);
        public List<RobocopyError> errors = new List<RobocopyError>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Robocopy Logs|*.log";
            if(fileDialog.ShowDialog() == true)
            {
                errors.Clear();
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += new DoWorkEventHandler(ProcessLog);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ProcessCompleted);
                _busyindicator.IsBusy = true;
                statusbarFile.Content = fileDialog.FileName;
                bw.RunWorkerAsync(argument: fileDialog.FileName);                
            }
        }
        public void ProcessLog(object sender, DoWorkEventArgs e)
        {
            string filename = (string)e.Argument;
            List<RobocopyError> errorlist = new List<RobocopyError>();
            using (FileStream fs = File.Open(filename, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (sr.Peek() > -1)
                    {
                        String line = sr.ReadLine();

                        Match matches = errorrx.Match(line);
                        if (matches.Success)
                        {
                            if (!line.Contains(@"desktop.ini") && !line.Contains(@"Thumbs.db") && !line.Contains(@"RECYCLE.BIN") && !line.Contains(@".etc"))
                            {
                                RobocopyError newerror = new RobocopyError();
                                newerror.Error = matches.Groups["error"].Value;
                                newerror.Path = matches.Groups["path"].Value;
                                errorlist.Add(newerror);
                            }
                        }
                    }
                }
            }
            e.Result = errorlist;
        }
        public void ProcessCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("Process Finished");
            _busyindicator.IsBusy = false;
            dgOutput.ItemsSource = (List<RobocopyError>)e.Result;
            statusbarErrors.Content = ((List<RobocopyError>)e.Result).Count;
            statusbarErrors.Visibility = Visibility.Visible;
            statusbarError.Visibility = Visibility.Visible;
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class RobocopyError
    {
        public string Error { get; set; }
        public string Path { get; set; }
        public RobocopyError()
        {
            Error = "";
            Path = "";
        }
    }
}
