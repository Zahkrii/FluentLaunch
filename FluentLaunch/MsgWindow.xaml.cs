using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FluentLaunch
{
    public enum MsgButton
    {
        OK,
        ConfirmCancel,
        Custom
    }

    public enum MsgImage
    {
        Info,
        Warning,
        Erorr
    }

    /// <summary>
    /// MsgWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MsgWindow : Window
    {
        public Messenger.DelegateMessage Callback;

        public MsgWindow(string msg, string title = null, MsgImage msgImage = MsgImage.Info, MsgButton msgButton = MsgButton.OK, string positiveBtnTxt = null, string negativeBtnTxt = null)
        {
            InitializeComponent();

            var accentCompositor = new WindowAccentCompositor(this);
            accentCompositor.Color = System.Windows.Media.Color.FromArgb(0x14, 0xff, 0xff, 0xff);
            accentCompositor.IsEnabled = true;

            TitleBlock.Text = title;
            MsgBlock.Text = msg;
            switch (msgImage)
            {
                case MsgImage.Info:
                    MsgImageBrush.ImageSource = ImageProcess.CreateBitmapSourceFromGdiBitmap(Properties.Resources.info);
                    break;

                case MsgImage.Erorr:
                    MsgImageBrush.ImageSource = ImageProcess.CreateBitmapSourceFromGdiBitmap(Properties.Resources.error);
                    break;

                case MsgImage.Warning:
                    MsgImageBrush.ImageSource = ImageProcess.CreateBitmapSourceFromGdiBitmap(Properties.Resources.warning);
                    break;

                default:
                    MsgImageBrush.ImageSource = ImageProcess.CreateBitmapSourceFromGdiBitmap(Properties.Resources.info);
                    break;
            }
            switch (msgButton)
            {
                case MsgButton.OK:
                    PositiveButton.Content = "OK";
                    NegativeButton.Visibility = Visibility.Collapsed;
                    break;

                case MsgButton.ConfirmCancel:
                    PositiveButton.Content = "Confirm";
                    NegativeButton.Visibility = Visibility.Visible;
                    NegativeButton.Content = "Cancel";
                    break;

                case MsgButton.Custom:
                    PositiveButton.Content = positiveBtnTxt;
                    NegativeButton.Visibility = Visibility.Visible;
                    NegativeButton.Content = negativeBtnTxt;
                    break;

                default:
                    PositiveButton.Content = "OK";
                    NegativeButton.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void PositiveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Callback != null)
            {
                Callback(true);
            }
            Close();
        }

        private void NegativeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Callback != null)
            {
                Callback(false);
            }
            Close();
        }
    }
}