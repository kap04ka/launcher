using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Diagnostics;
using System.Data;

namespace launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string rootPath;
        private string versionPath;
        private string zipPath;
        private string BuildPath;

        private string currentVersion;
        private string newVersion;
        View view;
        WebClient downloadClient;
        public MainWindow()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            versionPath = Path.Combine(rootPath, "Version.txt");
            currentVersion = File.ReadAllText(versionPath);
            zipPath = Path.Combine(rootPath, "Build.zip");
            BuildPath = rootPath + "\\Build";

            view = new View();
            DataContext = view;
            cancelButton.IsEnabled = false;

            curVer.Text = "Текущая версия: " + currentVersion;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (WebClient wc = new WebClient())
            {
                currentVersion = File.ReadAllText(versionPath);
                newVersion = wc.DownloadString("https://drive.google.com/uc?export=download&id=1I-ggSNmhWfuP8TKR58MhBcjbHPcMtbK8");

                if (int.Parse(newVersion.Replace(".", "")) > int.Parse(currentVersion.Replace(".", "")))
                {
                    var result = MessageBox.Show($"Доступна версия {newVersion}. Обновить?", "", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        uploadingBar.Visibility = Visibility.Visible;
                        uploadingSpeed.Visibility = Visibility.Visible;

                        cancelButton.IsEnabled = true;
                        updateButton.IsEnabled = false;

                        Upload();

                    }

                    else
                    {
                        StartApp();
                    }

                }
                else
                {
                    MessageBox.Show($"Версия актуальна");
                    StartApp();
                }

            }
        }

        public void Upload()
        {

            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += (s, e) => view.progress = e.ProgressPercentage;
                client.DownloadProgressChanged += (s, e) => view.speedCalculate(e.BytesReceived);
                client.DownloadFileCompleted += (s, e) =>
                {
                    if (e.Cancelled)
                    {
                        MessageBox.Show("Обновление прервано");
                        UpdateForm(false);
                        DeletedUndownloadFiles();
                        return;
                    }

                    UploadCompleted();
                    MessageBox.Show("Загрузка завершена");
                    UpdateForm(true);
                };

                client.DownloadFileAsync(new System.Uri("https://drive.google.com/uc?export=download&id=1wShEa8ju41uabVel8N0iHXuokWWyZdoi"), zipPath);

                downloadClient = client;
            }
        }

        public void DeletedUndownloadFiles()
        {
            File.Delete(zipPath);
        }

        public void UploadCompleted()
        {
            Directory.Delete(BuildPath, true);
            ZipFile.ExtractToDirectory(zipPath, rootPath);
            File.Delete(zipPath);
            File.WriteAllText(versionPath, newVersion);
            StartApp();
        }

        public void StartApp()
        {
            System.Diagnostics.Process.Start(BuildPath + "\\downloader.exe");
        }

        public void UpdateForm(bool newForm)
        {
            if(newForm) curVer.Text = "Текущая версия: " + newVersion;
            uploadingBar.Visibility = Visibility.Hidden;
            uploadingSpeed.Visibility = Visibility.Hidden;
            view.progress = 0;
            view.speed = $"Скорость: 0 Mb/s";

            cancelButton.IsEnabled = false;
            updateButton.IsEnabled = true;
        }

        private void cancellButton_Click(object sender, RoutedEventArgs e)
        {
            downloadClient.CancelAsync();
        }
    }
}