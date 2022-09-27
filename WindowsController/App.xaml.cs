using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WindowsController
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {

            base.OnStartup(e);

            //var menu = new System.Windows.Forms.ContextMenuStrip();
            //menu.Items.Add("終了", null, Exit_Click);

            //var notifyIcon = new System.Windows.Forms.NotifyIcon
            //{
            //    Visible = true,
            //    Icon = new System.Drawing.Icon(@"favicon.ico"),
            //    Text = "Windows Controller",
            //    ContextMenuStrip = menu
            //};

            //notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(NotifyIcon_Click);

            //int x = System.Windows.Forms.Cursor.Position.X;
            ////Y座標を取得する
            //int y = System.Windows.Forms.Cursor.Position.Y;

            ////マウスポインタの位置を画面左上（座標 (0, 0)）にする
            //System.Windows.Forms.Cursor.Position = new System.Drawing.Point(0, 0);
        }


        //private void NotifyIcon_Click(object sender, System.Windows.Forms.MouseEventArgs e)
        //{
        //    if (e.Button == System.Windows.Forms.MouseButtons.Left)
        //    {
        //        var wnd = new MainWindow();
        //        wnd.Show();
        //    }
        //}

        //private void Exit_Click(object sender, EventArgs e)
        //{
        //    Shutdown();
        //}
    }
}
