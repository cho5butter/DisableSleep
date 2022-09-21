using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace WindowsController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IntPtr hHook;

        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Minimized;
            this.Visibility = Visibility.Hidden;
            this.SetHook();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            MessageBox.Show("hiik");
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            MessageBox.Show("hiik");
            if (e.Key == Key.Escape)
            { Close(); }
        }

        private int SetHook()
        {
            IntPtr hmodule = WindowsAPI.GetModuleHandle(System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName);

            hHook = WindowsAPI.SetWindowsHookEx((int)WindowsAPI.HookType.WH_MOUSE_LL, (WindowsAPI.HOOKPROC)MyHookProc, hmodule, IntPtr.Zero);
            if (hHook == null)
            {
                MessageBox.Show("SetWindowsHookEx 失敗", "Error");
                return -1;
            }
            else
            {
                MessageBox.Show("SetWindowsHookEx 成功", "OK");
                return 0;
            }
        }

        private IntPtr MyHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (0 == WindowsAPI.HC_ACTION)
            {
                WindowsAPI.MSLLHOOKSTRUCT MouseHookStruct = (WindowsAPI.MSLLHOOKSTRUCT)System.Runtime.InteropServices.Marshal.PtrToStructure(lParam, typeof(WindowsAPI.MSLLHOOKSTRUCT));
                //MessageBox.Show(Convert.ToString(MouseHookStruct.pt.x));
            }

            return WindowsAPI.CallNextHookEx(hHook, nCode, wParam, lParam);

        }

    }
}
