using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MTGOReplayToolWpf
{
    public class Uploader
    {
        private ProgressBar ProgressBar { get; set; }

        public Uploader(ProgressBar progressBar)
        {
            PingDyno();
            ProgressBar = progressBar;
        }

        public void StartUpload()
        {
            var paths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.gz", SearchOption.TopDirectoryOnly).ToList();

            if (File.Exists("replays.zip"))
            {
                File.Delete("replays.zip");
            }

            ZipFile.CreateFromDirectory("replays", "replays.zip");
            paths.Add("replays.zip");

            paths.ForEach(x => UploadFile(x));
        }

        private void PingDyno()
        {
            using (var client = new WebClientEx())
            {
                client.Timeout = 600000;
                client.DownloadDataAsync(new Uri(@"https://pure-anchorage-19670.herokuapp.com/"));
            }
        }

        private void UploadFile(String path)
        {
            using (var client = new WebClientEx())
            {
                client.Timeout = 600000;
                client.UploadFileAsync(new Uri(@"https://pure-anchorage-19670.herokuapp.com/upload-replay-mort"), path);
                client.UploadProgressChanged += WebClientUploadProgressChanged;
                client.UploadFileCompleted += WebClientUploadCompleted;
            }
        }

        private void WebClientUploadCompleted(Object sender, UploadFileCompletedEventArgs e)
        {
            UpdateProgress(ProgressBar, 0);
        }

        private void WebClientUploadProgressChanged(Object sender, UploadProgressChangedEventArgs e)
        {
            UpdateProgress(ProgressBar, e.ProgressPercentage);
        }

        public void UpdateProgress(ProgressBar control, Int32 progress)
        {
            if (!control.Dispatcher.CheckAccess())
            {
                control.Dispatcher.Invoke(new ProgressBarUpdateDelegate(UpdateProgress), new Object[] { control, progress });  // invoking itself
            }
            else
            {
                if (progress < 50) control.Maximum = 55;
                else control.Maximum = Math.Min(progress + 5, 100);
                control.Visibility = Visibility.Visible;
                control.Value = progress;      // the "functional part", executing only on the main thread
            }
        }

        public delegate void ProgressBarUpdateDelegate(ProgressBar control, Int32 progress);  // defines a delegate type
    }

    class WebClientEx : WebClient
    {
        public Int32 Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            request.Timeout = Timeout;
            return request;
        }
    }
}
