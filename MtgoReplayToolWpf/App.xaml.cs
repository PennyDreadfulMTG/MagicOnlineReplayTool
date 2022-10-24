using Sentry;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace MtgoReplayToolWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            SentrySdk.Init(o =>
            {
                o.Dsn = "https://21c5c01f891647fc816e42f9177699d0@sentry.redpoint.games/9";

                // Set traces_sample_rate to 1.0 to capture 100% of transactions for performance monitoring.
                // We recommend adjusting this value in production.
                //o.TracesSampleRate = 1.0;

                o.IsGlobalModeEnabled = true;
            });
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        public static List<string> Args { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Args = new List<string>(e.Args);
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            SentrySdk.CaptureException(e.Exception);

            //e.Handled = true;
        }
    }
}
