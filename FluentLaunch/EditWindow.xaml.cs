using HandyControl.Controls;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace FluentLaunch
{
    /// <summary>
    /// EditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditWindow : System.Windows.Window
    {
        private ItemStored itemStored = new ItemStored();
        private Category _category = new Category();

        private enum IsIconSelected { Null, Native, Edited };

        private IsIconSelected isIconSelected = IsIconSelected.Null;

        private string filePath = null;
        private string iconPath = null;
        private bool isEditing = false;

        private IList<string> command = new List<string> { "Program", "CLSID", "CMD" };

        public EditWindow(Category category)
        {
            InitializeComponent();

            isEditing = false;

            var accentCompositor = new WindowAccentCompositor(this);
            accentCompositor.Color = Color.FromArgb(0x01, 0xff, 0xff, 0xff);
            accentCompositor.IsEnabled = true;

            IList<Category> categorys = XMLProcess.LoadCategory();
            CategoryBox.DataContext = categorys;
            CategoryBox.SelectedIndex = category.Id;

            CmdBox.DataContext = command;
            CmdBox.SelectedIndex = 0;
        }

        public EditWindow(Item item, Category category)
        {
            InitializeComponent();

            isEditing = true;
            isIconSelected = IsIconSelected.Edited;
            filePath = item.Target;

            var accentCompositor = new WindowAccentCompositor(this);
            accentCompositor.Color = Color.FromArgb(0x01, 0xff, 0xff, 0xff);
            accentCompositor.IsEnabled = true;

            IList<Category> categorys = XMLProcess.LoadCategory();
            CategoryBox.DataContext = categorys;
            CategoryBox.SelectedIndex = category.Id;

            TitleBox.Text = item.Title;
            itemStored.Title = item.Title;
            ToolTipBox.Text = item.ToolTip;
            itemStored.ToolTip = item.ToolTip;
            TargetBox.Text = item.Target;
            itemStored.Target = item.Target;
            IconShowBox.ImageSource = item.Icon;

            itemStored.Sort = item.Sort;

            CmdBox.DataContext = command;
            int i = 0;
            foreach (string cmd in command)
            {
                if (cmd == item.Command)
                {
                    CmdBox.SelectedIndex = i;
                }
                i++;
            }
            itemStored.Command = item.Command;
            EnableConfirmButton();
        }

        private void ImageEdit_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //创建一个打开文件式的对话框
            OpenFileDialog ofd = new OpenFileDialog();
            //设置这个对话框的起始打开路径
            ofd.InitialDirectory = @"D:\";
            //设置打开的文件的类型，注意过滤器的语法
            ofd.Filter = "All|*.ico;*.png;*.jpg|Icon|*.ico|PNG|*.png|JPG|*.jpg";
            //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
            if (ofd.ShowDialog() == true)
            {
                if (ImageProcess.GetIcon(ofd.FileName, 128, 128) != null)
                {
                    IconShowBox.ImageSource = ImageProcess.GetIcon(ofd.FileName, 128, 128);
                    iconPath = ofd.FileName;
                    isIconSelected = IsIconSelected.Edited;
                }
                else
                {
                    HandyControl.Controls.MessageBox.Show("There is no icon in the exe file,please select another one.", null, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            EnableConfirmButton();
        }

        private void Box_TextChanged(object sender, TextChangedEventArgs e)
        {
            itemStored.Title = TitleBox.Text;
            itemStored.ToolTip = ToolTipBox.Text;
            itemStored.Target = TargetBox.Text;

            EnableConfirmButton();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            itemStored.Title = TitleBox.Text;
            itemStored.Target = TargetBox.Text;
            if (string.IsNullOrEmpty(itemStored.ToolTip) || string.IsNullOrWhiteSpace(itemStored.ToolTip))
            {
                itemStored.ToolTip = itemStored.Title;
            }

            //TODO: Debug
            //保存图标
            if (isIconSelected == IsIconSelected.Native)
            {
                // exe程序的文件名
                //string name = filepath.Split('\\').Last().Split('.').First();
                itemStored.Icon = ImageProcess.SaveIcon(filePath, 128, 128, TitleBox.Text);
            }
            else if (isIconSelected == IsIconSelected.Edited)
            {
                itemStored.Icon = ImageProcess.SaveIcon(iconPath, 128, 128, TitleBox.Text);
            }

            if (isEditing)
            {
                XMLProcess.UpdateLayoutItem(itemStored, _category);
            }
            else
            {
                XMLProcess.AddLayoutItem(itemStored, _category);
            }
            Close();
        }

        private void EnableConfirmButton()
        {
            if (!string.IsNullOrEmpty(itemStored.Title) && !string.IsNullOrEmpty(itemStored.Target)
                && !string.IsNullOrWhiteSpace(itemStored.Title) && !string.IsNullOrWhiteSpace(itemStored.Target)
                && isIconSelected != IsIconSelected.Null)
            {
                // 当title、target、icon都有值
                ConfirmButton.IsEnabled = true;
            }
        }

        private void CategoryBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _category = (Category)CategoryBox.SelectedItem;
            EnableConfirmButton();
        }

        private void CmdBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            itemStored.Command = (string)CmdBox.SelectedItem;
            EnableConfirmButton();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            //创建一个打开文件式的对话框
            OpenFileDialog ofd = new OpenFileDialog();
            //设置这个对话框的起始打开路径
            ofd.InitialDirectory = @"D:\";
            //设置打开的文件的类型，注意过滤器的语法
            ofd.Filter = "EXE|*.exe";
            //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
            if (ofd.ShowDialog() == true)
            {
                if (ImageProcess.GetIcon(ofd.FileName, 256, 256) != null)
                {
                    IconShowBox.ImageSource = ImageProcess.GetIcon(ofd.FileName, 256, 256);
                    isIconSelected = IsIconSelected.Native;
                }

                filePath = ofd.FileName;

                TargetBox.Text = ofd.FileName;
                TitleBox.Text = ofd.SafeFileName.Split('.').First();
                ToolTipBox.Text = TitleBox.Text;
            }
        }
    }
}