using System;
using System.Windows;
using System.Windows.Controls;

namespace MtgoReplayToolWpf
{
    public static class UiHelper
    {
        public static MainWindow MainWindow {get; set;}

        public static Double Progress { get; set; }

        public static Double Maximum { get; set; }

        public static void LockUi()
        {
            MainWindow.SetLock(true);
        }

        public static void UnlockUi()
        {
            MainWindow.SetLock(false);
        }

        public static void AddProgress(Double increment)
        {
            Progress += increment;
            UpdateProgress(MainWindow.ProgressBar, Progress, Maximum);
        }

        public static void SetProgress(Double progress)
        {
            UpdateProgress(MainWindow.ProgressBar, progress, 1.0);
        }

        public static void UpdateProgress(ProgressBar control, Double progress, Double progressMax)
        {
            if (!control.Dispatcher.CheckAccess())
            {
                control.Dispatcher.Invoke(new ProgressBarUpdateDelegate(UpdateProgress), control, progress, progressMax);
            }
            else
            {
                if (control.IsIndeterminate)
                {
                    control.IsIndeterminate = false;
                }

                control.Maximum = progressMax;
                control.Visibility = Visibility.Visible;
                control.Value = progress;
            }
        }

        public delegate void ProgressBarUpdateDelegate(ProgressBar control, Double progress, Double progressMax);
    }
}
