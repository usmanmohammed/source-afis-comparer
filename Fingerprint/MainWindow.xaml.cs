using Microsoft.Win32;
using SourceAFIS.Simple;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace Fingerprint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private AfisEngine Afis;
        private string FileAddress;
        private string imageurl;
        private string progressvalue;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        public string ImageUrl
        {
            get
            {
                return imageurl;
            }
            set
            {
                if (value != this.imageurl)
                {
                    imageurl = value;
                    NotifyPropertyChanged("ImageUrl");
                    Refresh(image1);
                }
            }
        }
        public string ProgressValue
        {
            get
            {
                return progressvalue;
            }
            set
            {
                if (value != this.progressvalue)
                {
                    progressvalue = value;
                    NotifyPropertyChanged("ProgressValue");
                    Refresh(progressBar);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var file = new OpenFileDialog();
            file.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;...";

            var result = file.ShowDialog();
            if (result == false)
            {
                return;
            }

            image.Source = new BitmapImage(new Uri(file.FileName));
            FileAddress = file.FileName;
        }
        public void Refresh(UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        }
        private async void button_Click(object sender, RoutedEventArgs e)
        {
            var resources = new OpenFileDialog();
            resources.Multiselect = true;
            resources.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;...";


            var result = resources.ShowDialog();
            if (result == false)
            {
                return;
            }

            string[] files = resources.FileNames;

            // Matching features
            Afis = new AfisEngine();
            Afis.Threshold = 10;

            List<MyPerson> Database = new List<MyPerson>();

            foreach (var file in files)
            {
                Database.Add(Enroll(file, new FileInfo(file).Name));
            }

            MyPerson probe = Enroll(FileAddress, new FileInfo(FileAddress).Name);

            Dictionary<float, MyPerson> dict = new Dictionary<float, MyPerson>();

            //BackgroundWorker worker = new BackgroundWorker();
            ////worker.WorkerReportsProgress = true;
            ////worker.DoWork += worker_DoWork;
            ////worker.ProgressChanged += worker_ProgressChanged;

            //worker.RunWorkerAsync();
            progressBar.Value = 0;
            int count = 1;

            foreach (var item in Database)
            {
                //await Task.Run(() => updateImage(item.FingerUrl));
                image1.Source = new BitmapImage(new Uri(item.FingerUrl));
                ImageUrl = item.FingerUrl;
                image1.DataContext = ImageUrl;
                Refresh(image1);

                var value = (double)100 * ((double)(Database.Count() - (Database.Count() - count)) / Database.Count());

                progressBar.Value = value;
                progressBar.DataContext = value.ToString();
                Refresh(progressBar);
                //BitmapImage _image = new BitmapImage();
                //_image.BeginInit();
                //_image.UriSource = new Uri("pack://application:,,," + item.FingerUrl);
                //_image.EndInit();
                //image1.Source = _image;

                float match = Afis.Verify(probe, item);

                item.MatchValue = match;

                dict.Add(count, item);

                //if (!dict.ContainsKey(match))
                //{
                //    dict.Add(match, item);
                //}
                //else
                //{
                //    match += 0.1f;
                //    dict.Add(match, item);

                //}

                Thread.Sleep(100);
                count++;
            }

            float highest = 0;
            foreach (var item in dict)
            {

                if (highest == 0)
                {
                    highest = item.Value.MatchValue;
                }

                else
                {
                    if (item.Value.MatchValue > highest)
                    {
                        highest = item.Value.MatchValue;
                    }
                }
            }

            var final = dict.Where(g => g.Value.MatchValue == highest).FirstOrDefault();

            Thread.Sleep(500);
            await Dispatcher.BeginInvoke((Action)(() => image1.Source = new BitmapImage(new Uri(final.Value.FingerUrl))));

            MessageBox.Show(string.Format("Match Found! {0} with similarity ratio of: {1}", final.Value.Name, final.Key));
        }

        private void updateImage(string url)
        {
            image1.Source = new BitmapImage(new Uri(url));
        }
        private MyPerson Enroll(string filename, string name)
        {
            //Console.WriteLine("Enrolling {0}...", name);

            // Initialize empty fingerprint object and set properties
            FingerprintObject fp = new FingerprintObject();
            fp.FileName = filename;
            // Load image from the file
            //Console.WriteLine(" Loading image from {0}...", filename);
            BitmapImage image = new BitmapImage(new Uri(filename, UriKind.RelativeOrAbsolute));
            fp.AsBitmapSource = image;
            // Above update of fp.AsBitmapSource initialized also raw image in fp.Image
            // Check raw image dimensions, Y axis is first, X axis is second
            //Console.WriteLine(" Image size = {0} x {1} (width x height)", fp.Image.GetLength(1), fp.Image.GetLength(0));

            // Initialize empty person object and set its properties
            MyPerson person = new MyPerson();
            person.Name = name;
            person.FingerUrl = filename;
            // Add fingerprint to the person
            person.Fingerprints.Add(fp);

            // Execute extraction in order to initialize fp.Template
            //Console.WriteLine(" Extracting template...");
            Afis.Extract(person);
            // Check template size
            //Console.WriteLine(" Template size = {0} bytes", fp.Template.Length);

            return person;
        }

    }

}
