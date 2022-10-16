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
    /// https://ameblo.jp/mojeld/entry-12731540496.html
    /// </summary>
    public partial class MainWindow : Window
    {
        private InterceptKeyboard _interceptKeyboard;
        private MouseHook _mouseHook;
        private System.Timers.Timer _timer;
        private ulong _elapsedTime;
        private int _moveInterval = 60;
        private int _moveCount = 200;
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
            this._interceptKeyboard = new InterceptKeyboard();
            this._interceptKeyboard.KeyDownEvent += InterceptKeyboard_KeyDownEvent;
            this._interceptKeyboard.KeyUpEvent += InterceptKeyboard_KeyUpEvent;
            this._interceptKeyboard.Hook();

            this._mouseHook = new MouseHook();
            this._mouseHook.MouseMoveEvent += this.Hook_MouseMoveEvent;
            this._mouseHook.Hook();

        }

        private void InterceptKeyboard_KeyUpEvent(object sender, InterceptKeyboard.OriginalKeyEventArg e)
        {
            System.Diagnostics.Debug.WriteLine("Keyup KeyCode {0}", e.KeyCode);
            this._elapsedTime = 0;
        }

        private void InterceptKeyboard_KeyDownEvent(object sender, InterceptKeyboard.OriginalKeyEventArg e)
        {
            System.Diagnostics.Debug.WriteLine("Keydown KeyCode {0}", e.KeyCode);
            this._elapsedTime = 0;
        }

        private void Hook_MouseMoveEvent(object sender, MouseEventArg e)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("X={0}, Y={1}", e.Point.X, e.Point.Y));
            this._elapsedTime = 0;
        }

        private void timerSet()
        {
            //タイマーセット
            this._timer = new System.Timers.Timer(1000);
            this._timer.Elapsed += timerEvent;
            this._timer.AutoReset = true;
            this._timer.Enabled = true;

            this._elapsedTime = 0;

            this._timer.Stop();
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
                Text = "停止中",
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

        private void timerEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            //指定秒経てばここが実行
            try
            {
                this._notifyIcon.Text = this._timer.Enabled ? "作動中" : "停止中";
                this._elapsedTime += 1;
                if (this._elapsedTime < (ulong)this._moveInterval) return;
                Random random = new Random();
                int ranTime = random.Next(50, 100);
                this._elapsedTime = 0;
                //AFK防止
                this.preventionAFK();
                //インターバル変更
                this._moveInterval =  ranTime;
            } catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return;
            }
        }

        private void preventionAFK()
        {

            System.Diagnostics.Debug.WriteLine(
                    "実行された；" + "待機時間" + this._moveInterval.ToString()
                );

            Random random = new Random();
            int ranMove = random.Next(100, 500);
            this._moveCount = ranMove;

            for (int i = 0; i < this._moveCount; i++)
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

            //キーボード送信
            System.Windows.Forms.SendKeys.SendWait("^+%");

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

        protected override void OnClosing(CancelEventArgs e)
        {
            //終了時にフックを外す
            base.OnClosing(e);
            this._interceptKeyboard.UnHook();
            this._mouseHook.UnHook();
        }

        private void enableAutoLunch(object sender, EventArgs e)
        {
            try
            {
                //スタートアップフォルダと実行ファイルのパス取得
                string shortcutPath = Environment.GetFolderPath(System.Environment.SpecialFolder.Startup) + "\\WinSys.lnk";
                string targetPath = System.Windows.Forms.Application.ExecutablePath;

                System.Diagnostics.Debug.WriteLine(shortcutPath);
                System.Diagnostics.Debug.WriteLine(targetPath);

                //ショートカット作成
                IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);

                shortcut.TargetPath = targetPath;
                shortcut.WorkingDirectory = System.Windows.Forms.Application.StartupPath;
                shortcut.Description = "テストのアプリケーション";
                shortcut.Save();

                //後処理
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            ////レジストリ登録
            //try
            //{
            //    //Runキーを開く
            //    Microsoft.Win32.RegistryKey regkey =
            //        Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
            //        @"Software\Microsoft\Windows\CurrentVersion\Run", true);
            //    //値の名前に製品名、値のデータに実行ファイルのパスを指定し、書き込む
            //    regkey.SetValue(System.Windows.Forms.Application.ProductName, System.Windows.Forms.Application.ExecutablePath);
            //    //閉じる
            //    regkey.Close();
            //    System.Diagnostics.Debug.WriteLine(System.Windows.Forms.Application.ProductName);
            //    System.Diagnostics.Debug.WriteLine(System.Windows.Forms.Application.ExecutablePath);
            //}
            //catch (Exception ex)
            //{
            //    //MessageBox.Show(ex.Message);
            //    System.Diagnostics.Debug.WriteLine(ex.Message);
            //}
        }

        private void disableAutoLunch(object sender, EventArgs e)
        {
            try
            {
                string shortcutPath = Environment.GetFolderPath(System.Environment.SpecialFolder.Startup) + "\\WinSys.lnk";
                System.IO.File.Delete(shortcutPath);
            } catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            ////レジストリ登録削除
            //try
            //{
            //    Microsoft.Win32.RegistryKey regkey =
            //        Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
            //        @"Software\Microsoft\Windows\CurrentVersion\Run", true);
            //    regkey.DeleteValue(System.Windows.Forms.Application.ProductName, false);
            //    regkey.Close();
            //}
            //catch(Exception ex)
            //{
            //    //MessageBox.Show(ex.Message);
            //    System.Diagnostics.Debug.WriteLine(ex.Message);
            //}
        }

    }
}
