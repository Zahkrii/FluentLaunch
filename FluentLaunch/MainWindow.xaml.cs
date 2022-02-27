using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FluentScheduler;

namespace FluentLaunch
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        // 存储分类的列表
        private IList<Category> categorylist = new ObservableCollection<Category>();

        // 此时显示的分类
        private Category presentCategory = new Category();

        // 选中的item
        private Item selectedItem = new Item();

        // 通知栏图标
        private System.Windows.Forms.NotifyIcon TrayIcon = new System.Windows.Forms.NotifyIcon();

        // 程序运行目录
        private static string _path = Directory.GetCurrentDirectory() + "\\Assets";

        /// <summary>
        /// 初始化数据，从 Assets/layout.xml 读取列表
        /// </summary>
        private void InitData()
        {
            categorylist = XMLProcess.LoadCategory();
            presentCategory = categorylist.First();
            IList<Item> _items = new ObservableCollection<Item>();
            _items = XMLProcess.LoadLayout(presentCategory.Name);
            MainLayout.DataContext = _items;
        }

        private void InitNotifyIcon()
        {
            TrayIcon.BalloonTipText = "Fluent Launch 已启动"; //设置程序启动时显示的文本
            TrayIcon.Text = "Fluent Launch";
            TrayIcon.Visible = true;
            TrayIcon.ShowBalloonTip(1);

            //tray menu
            //TrayIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(notifyIcon_MouseClick);

            System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();
            System.Windows.Forms.MenuItem menuItem1 = new System.Windows.Forms.MenuItem("Fluent Launch");
            menuItem1.Enabled = false;
            menu.MenuItems.Add(menuItem1);

            System.Windows.Forms.MenuItem breakItem = new System.Windows.Forms.MenuItem("-");
            menu.MenuItems.Add(breakItem);

            System.Windows.Forms.MenuItem menuItem2 = new System.Windows.Forms.MenuItem("Show");
            menuItem2.Click += new EventHandler(showWindow);
            menu.MenuItems.Add(menuItem2);

            System.Windows.Forms.MenuItem menuItem3 = new System.Windows.Forms.MenuItem("Settings");
            //menuItem3.Click += new EventHandler(Exit);
            menu.MenuItems.Add(menuItem3);

            System.Windows.Forms.MenuItem menuItem4 = new System.Windows.Forms.MenuItem("Exit");
            menuItem4.Click += new EventHandler(exitApp);
            menu.MenuItems.Add(menuItem4);

            TrayIcon.ContextMenu = menu;
        }

        private void exitApp(object sender, EventArgs e)
        {
            TrayIcon.Visible = false;
            Close();
        }

        private void showWindow(object sender, EventArgs e)
        {
            revealWindow();
        }

        //private void notifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        //{
        //    if (e.Button == System.Windows.Forms.MouseButtons.Right)
        //    {
        //        ContextMenu NotifyIconMenu = (ContextMenu)FindResource("NotifyIconMenu");
        //        NotifyIconMenu.IsOpen = true;
        //        //Application.Current.MainWindow.Activate();
        //        //Application.Current.Deactivated += app_Deactivated;
        //    }
        //}

        //private void app_Deactivated(object sender, EventArgs e)
        //{
        //    ContextMenu NotifyIconMenu = (ContextMenu)FindResource("NotifyIconMenu");
        //    if (NotifyIconMenu.IsOpen == true)
        //    {
        //        NotifyIconMenu.IsOpen = false;
        //    }
        //}

        private void changeTheme()
        {
            int hour;
            int.TryParse(DateTime.Now.ToString("HH"), out hour);

            JobManager.Initialize();

            if (hour > 7 && hour < 19)
            {
                switchToLightMode();
                JobManager.AddJob(
                () =>
                {
                    System.Windows.Forms.Application.Restart();
                    Application.Current.Shutdown();
                },
                s => s.ToRunOnceAt(19, 0)
                );
            }
            else
            {
                switchToDarkMode();
                JobManager.AddJob(
                () =>
                {
                    System.Windows.Forms.Application.Restart();
                    Application.Current.Shutdown();
                },
                s => s.ToRunOnceAt(7, 0)
                );
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            changeTheme();

            InitNotifyIcon();

            try
            {
                InitData();
            }
            catch
            {
                //XMLProcess.CreateXMLFile();
            }

            Hotkey.Regist(this, HotkeyModifiers.MOD_ALT, Key.S, () =>
            {
                if (this.Visibility == Visibility.Visible)
                    hideWindow();
                else
                    revealWindow();
            });
            hideWindow();
        }

        private void switchToDarkMode()
        {
            var accentCompositor = new WindowAccentCompositor(this);
            accentCompositor.Color = Color.FromArgb(0x14, 0x00, 0x00, 0x00);
            accentCompositor.IsEnabled = true;
            MainLayout.ItemContainerStyle = (Style)Application.Current.Resources["ListBoxItemDarkStyle"];
            MainLayout.ItemTemplate = (DataTemplate)Application.Current.Resources["ListBoxDarkStyle"];
            MainLaunchWindow.Template = (ControlTemplate)Application.Current.Resources["WindowDark"];
            MainLaunchWindow.Icon = ImageProcess.CreateBitmapSourceFromGdiBitmap(Properties.Resources.icon_light.ToBitmap());
            TrayIcon.Icon = Properties.Resources.icon_light;
            TitleBlock.Foreground = Brushes.White;
            PreviousText.Foreground = Brushes.White;
            NextText.Foreground = Brushes.White;
        }

        private void switchToLightMode()
        {
            var accentCompositor = new WindowAccentCompositor(this);
            accentCompositor.Color = Color.FromArgb(0x14, 0xff, 0xff, 0xff);
            accentCompositor.IsEnabled = true;
            MainLayout.ItemContainerStyle = (Style)Application.Current.Resources["ListBoxItemLightStyle"];
            MainLayout.ItemTemplate = (DataTemplate)Application.Current.Resources["ListBoxLightStyle"];
            MainLaunchWindow.Template = (ControlTemplate)Application.Current.Resources["WindowLight"];
            MainLaunchWindow.Icon = ImageProcess.CreateBitmapSourceFromGdiBitmap(Properties.Resources.icon_dark.ToBitmap());
            TrayIcon.Icon = Properties.Resources.icon_dark;
            TitleBlock.Foreground = Brushes.Black;
            PreviousText.Foreground = Brushes.Black;
            NextText.Foreground = Brushes.Black;
        }

        private void hideWindow()
        {
            GlobalAnimationTriger.IsChecked = false;
            Hide();
        }

        private void revealWindow()
        {
            Show();
            GlobalAnimationTriger.IsChecked = true;
        }

        private void CreateRightMenu_Click(object sender, RoutedEventArgs e)
        {
            EditWindow createWindow = new EditWindow(presentCategory);
            createWindow.Owner = this;
            createWindow.ShowDialog();
            RefreashLayout(presentCategory.Name);
        }

        private void MainLayout_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            selectedItem = (Item)MainLayout.SelectedItem;
            if (selectedItem != null)
            {
                ItemTitle.Header = selectedItem.ToolTip;
                MainLayout.SelectedItem = null;
            }
        }

        private void MainLayout_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Item selectedItem = (Item)MainLayout.SelectedItem;
            if (selectedItem != null)
            {
                switch (selectedItem.Command)
                {
                    case "CLSID":
                        Process.Start("explorer.exe", selectedItem.Target);
                        break;

                    case "Program":
                        Process.Start(selectedItem.Target);
                        break;

                    default:
                        break;
                }
                MainLayout.SelectedItem = null;
                hideWindow();
            }
        }

        private void Rectangle_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                //向上滚动滑轮
                PreviousSection();
            }
            else
            {
                //向下滚动滑轮
                NextSection();
            }
        }

        private void PreviousSection()
        {
            var tmpList = from cate in categorylist
                          where cate.Id < presentCategory.Id
                          select cate;
            try
            {
                presentCategory = tmpList.Last();
            }
            catch
            {
                presentCategory = categorylist.Last();
            }
            TitleBlock.Text = presentCategory.Name;
            RefreashLayout(presentCategory.Name);
        }

        private void NextSection()
        {
            var tmpList = from cate in categorylist
                          where cate.Id > presentCategory.Id
                          select cate;
            try
            {
                presentCategory = tmpList.First();
            }
            catch
            {
                presentCategory = categorylist.First();
            }
            TitleBlock.Text = presentCategory.Name;
            RefreashLayout(presentCategory.Name);
        }

        private void RefreashLayout(string category)
        {
            MainLayout.DataContext = null;
            MainLayout.DataContext = XMLProcess.LoadLayout(category);
        }

        private void EditRightMenu_Click(object sender, RoutedEventArgs e)
        {
            EditWindow editWindow = new EditWindow(selectedItem, presentCategory);
            editWindow.Owner = this;
            editWindow.ShowDialog();
            RefreashLayout(presentCategory.Name);
        }

        private void MinimumRightMenu_Click(object sender, RoutedEventArgs e)
        {
            hideWindow();
        }

        #region TODO：拖拽排序

        //private void MainLayout_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    var pos = e.GetPosition(MainLayout);
        //    HitTestResult result = VisualTreeHelper.HitTest(MainLayout, pos);
        //    if (result == null)
        //    {
        //        return;
        //    }
        //    var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
        //    if (listBoxItem == null || listBoxItem.Content != MainLayout.SelectedItem)
        //    {
        //        return;
        //    }
        //    DataObject dataObj = new DataObject(listBoxItem.Content as Item);
        //    DragDrop.DoDragDrop(MainLayout, dataObj, DragDropEffects.Move);
        //}

        //private void MainLayout_Drop(object sender, DragEventArgs e)
        //{
        //    var pos = e.GetPosition(MainLayout);
        //    var result = VisualTreeHelper.HitTest(MainLayout, pos);
        //    if (result == null)
        //    {
        //        return;
        //    }
        //    //查找元数据
        //    var sourcePerson = e.Data.GetData(typeof(Item)) as Item;
        //    if (sourcePerson == null)
        //    {
        //        return;
        //    }
        //    //查找目标数据
        //    var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
        //    if (listBoxItem == null)
        //    {
        //        return;
        //    }
        //    var targetPerson = listBoxItem.Content as Item;
        //    if (ReferenceEquals(targetPerson, sourcePerson))
        //    {
        //        return;
        //    }
        //    _items.Remove(sourcePerson);
        //    _items.Insert(_items.IndexOf(targetPerson), sourcePerson);
        //}

        #endregion TODO：拖拽排序

        private void DelRightMenu_Click(object sender, RoutedEventArgs e)
        {
            MsgWindow msgWindow = new MsgWindow("Confirm Delete? This can not to be undone.", "Warning", MsgImage.Warning, MsgButton.ConfirmCancel);
            msgWindow.Owner = this;
            msgWindow.Callback = DelCallBack;
            msgWindow.ShowDialog();
            RefreashLayout(presentCategory.Name);
        }

        private void DelCallBack(bool result)
        {
            if (result)
            {
                XMLProcess.DeleteLayoutItem(selectedItem.Sort, presentCategory);
                RefreashLayout(presentCategory.Name);
            }
            else
            {
            }
        }

        private void SortRightMenu_Click(object sender, RoutedEventArgs e)
        {
            //SortingWindow sortingWindow = new SortingWindow();
            //sortingWindow.Owner = this;
            //sortingWindow.ShowDialog();
            XMLProcess.ResortXML();
        }

        private void SettingRightMenu_Click(object sender, RoutedEventArgs e)
        {
        }

        private void PreviousRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PreviousSection();
        }

        private void NextRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            NextSection();
        }
    }
}