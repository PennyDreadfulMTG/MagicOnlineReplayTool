using System;
using System.Collections.Generic;
using System.Windows;

namespace MtgoReplayToolWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static List<String> Args { get; set; }

        private void Application_Startup(System.Object sender, StartupEventArgs e)
        {
            Args = new List<String>(e.Args);
        }
    }
}
