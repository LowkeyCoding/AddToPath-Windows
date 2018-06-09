using System;
using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Diagnostics.Contracts;
using System.IO;
using System.Windows.Documents;
using System.Text.RegularExpressions;

namespace AddToPath
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Path p = new Path();
            string path = new TextRange(TextboxPath.Document.ContentStart, TextboxPath.Document.ContentEnd).Text;
            string[] rPath = path.Split(new[] { Environment.NewLine },StringSplitOptions.None);
            p.Check(rPath.Length,rPath);
         }
    }
    public class Path
    {
        const int HWND_BROADCAST = 0xffff;
        const uint WM_SETTINGCHANGE = 0x001a;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SendNotifyMessage(IntPtr hWnd, uint Msg, UIntPtr wParam, string lParam);
        //add to path
        public void addToPath(string path)
        {
            using (var envKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment",true))
            {
                Contract.Assert(envKey != null, @"registry key is missing!");
                envKey.SetValue("Path", envKey.GetValue("Path") + ";" + path);
                SendNotifyMessage((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE,
                    (UIntPtr)0, "Environment");
            }
        }
        //add multiple items to path
        public int multiAddToPath(int loops, string[] args)
        {
            if (loops < 1)
            {
                return 1;
            }
            addToPath(args[loops-1]);
            return multiAddToPath(loops - 1, args);
        }
        public int Check (int loops, string[] args)
        {
            string messageBoxText = "Do you want to add these to Path?" + Environment.NewLine + stringArrayToString(args, args.Length);
            string caption = "Add to Path";
            MessageBoxButton button = MessageBoxButton.YesNoCancel;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    return multiAddToPath(loops, args);
                    break;
                case MessageBoxResult.No:
                    return 1;
                    break;
                case MessageBoxResult.Cancel:
                    return 1;
                    break;
            }
            return 1;
        }
        public string stringArrayToString(string[] args, int loops, string accum="")
        {
            if (loops < 1)
                return accum;

            accum += args[loops - 1] + Environment.NewLine;
            return stringArrayToString(args, loops-1, accum);
        }
    }

}
