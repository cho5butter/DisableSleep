using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// https://lets-csharp.com/keyboard-hook/
    /// https://www.ipentec.com/document/csharp-get-mouse-pointer-screen-position-using-global-hook
    /// </summary>
    public partial class MainWindow : Window
    {
        private IntPtr _mouseHook;
        private KeyboardHook _keybordHook;
        private System.Timers.Timer _timer;
        private ulong _elapsedTime;
        private const int _moveInterval = 60;
        private const int _moveCount = 50;
        private System.Windows.Forms.ContextMenuStrip _contextMenu;
        private System.Windows.Forms.NotifyIcon _notifyIcon;

        public MainWindow()
        {
            InitializeComponent();

            //Windowを隠す
            this.WindowState = WindowState.Minimized;
            this.Visibility = Visibility.Hidden;

            this.hookSet();
            this.timerSet();
            this.menuSet();

        }

        private void hookSet()
        {
            //マウスフックセット
            this.SetHook();

            //キーボードフックセット
            this._keybordHook = new KeyboardHook();
            this._keybordHook.KeyDownEvent += KeyboardHook_KeyDownEvent;
            this._keybordHook.Hook();
        }

        private void timerSet()
        {
            //タイマーセット
            this._timer = new System.Timers.Timer(1000);
            this._timer.Elapsed += timerEvent;
            this._timer.AutoReset = true;
            this._timer.Enabled = true;

            this._elapsedTime = 0;

            this._timer.Start();
        }

        private void menuSet()
        {
            //メニュー設定
            this._contextMenu = new System.Windows.Forms.ContextMenuStrip();
            this._contextMenu.Items.Add("一時停止", null, timerPause);
            this._contextMenu.Items.Add("再開", null, timerRestart);
            this._contextMenu.Items.Add("自動起動有効", null, enableAutoLunch);
            this._contextMenu.Items.Add("自動起動無効", null, disableAutoLunch);
            this._contextMenu.Items.Add("終了", null, Exit_Click);

            this._notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Visible = true,
                Icon = new System.Drawing.Icon(@"favicon.ico"),
                Text = "作動中",
                ContextMenuStrip = this._contextMenu
            };
        }

        private void timerPause(object sender, EventArgs e)
        {
            //作動一時停止
            if (!this._timer.Enabled) return;
            this._timer.Stop();
            this._notifyIcon.Text = "停止中";

        }

        private void timerRestart(object sender, EventArgs e)
        {
            //作動再開
            if (this._timer.Enabled) return;
            this._timer.Start();
            this._notifyIcon.Text = "作動中";
        }

        private void KeyboardHook_KeyDownEvent(object sender, KeyEventArg e)
        {
            // キーが押されたときにやりたいこと
            this._elapsedTime = 0;
        }

        private void MouseMove(WindowsAPI.MSLLHOOKSTRUCT MouseHookStruct)
        {
            //マウスが動いたときにやりたいこと
            this._elapsedTime = 0;
        }

        private void timerEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            //指定秒経てばここが実行
            try
            {
                this._notifyIcon.Text = this._timer.Enabled ? "作動中" : "停止中";
                this._elapsedTime += 1;
                if (this._elapsedTime < _moveInterval) return;
                this._elapsedTime = 0;
                //AFK防止
                this.preventionAFK();
            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return;
            }
        }

        private void preventionAFK()
        {
            for (int i = 0; i < _moveCount; i++)
            {
                //Mouse動かす
                int mouseX = System.Windows.Forms.Cursor.Position.X;
                int mouseY = System.Windows.Forms.Cursor.Position.Y;

                Random rand = new Random();
                int moveX = rand.Next(minValue: -1, maxValue: 1);
                int moveY = rand.Next(minValue: -1, maxValue: 1);

                try
                {
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point(mouseX + moveX, mouseY + moveY);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            }

        }

        private void NotifyIcon_Click(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var wnd = new MainWindow();
                wnd.Show();
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            //アプリ終了
            this.Close();
        }

        private int SetHook()
        {
            IntPtr hmodule = WindowsAPI.GetModuleHandle(System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName);

            _mouseHook = WindowsAPI.SetWindowsHookEx((int)WindowsAPI.HookType.WH_MOUSE_LL, (WindowsAPI.HOOKPROC)MyHookProc, hmodule, IntPtr.Zero);
            if (_mouseHook == null)
            {
                System.Diagnostics.Debug.WriteLine("Set Hook失敗");
                MessageBox.Show("起動に失敗しました", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return -1;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Set Hook成功");
                return 0;
            }
        }

        private IntPtr MyHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (0 == WindowsAPI.HC_ACTION)
            {
                WindowsAPI.MSLLHOOKSTRUCT MouseHookStruct = (WindowsAPI.MSLLHOOKSTRUCT)System.Runtime.InteropServices.Marshal.PtrToStructure(lParam, typeof(WindowsAPI.MSLLHOOKSTRUCT));
                //MessageBox.Show(Convert.ToString(MouseHookStruct.pt.x));
                this.MouseMove(MouseHookStruct);
            }

            return WindowsAPI.CallNextHookEx(_mouseHook, nCode, wParam, lParam);

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //終了時にフックを外す
            base.OnClosing(e);
            this._keybordHook.UnHook();
            WindowsAPI.UnhookWindowsHookEx(_mouseHook);
        }

        private void enableAutoLunch(object sender, EventArgs e)
        {
            //レジストリ登録
            try
            {
                //Runキーを開く
                Microsoft.Win32.RegistryKey regkey =
                    Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", true);
                //値の名前に製品名、値のデータに実行ファイルのパスを指定し、書き込む
                regkey.SetValue(System.Windows.Forms.Application.ProductName, System.Windows.Forms.Application.ExecutablePath);
                //閉じる
                regkey.Close();
                System.Diagnostics.Debug.WriteLine(System.Windows.Forms.Application.ProductName);
                System.Diagnostics.Debug.WriteLine(System.Windows.Forms.Application.ExecutablePath);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void disableAutoLunch(object sender, EventArgs e)
        {
            //レジストリ登録削除
            try
            {
                Microsoft.Win32.RegistryKey regkey =
                    Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", true);
                regkey.DeleteValue(System.Windows.Forms.Application.ProductName, false);
                regkey.Close();
            }
            catch(Exception ex)
            {
                //MessageBox.Show(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

    }
}
