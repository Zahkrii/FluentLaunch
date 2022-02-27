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
using System.Windows.Shapes;

namespace FluentLaunch
{
    /// <summary>
    /// SortingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SortingWindow : Window
    {
        public SortingWindow()
        {
            InitializeComponent();

            var accentCompositor = new WindowAccentCompositor(this);
            accentCompositor.Color = Color.FromArgb(0x14, 0xff, 0xff, 0xff);
            accentCompositor.IsEnabled = true;
        }
    }
}