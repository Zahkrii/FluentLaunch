using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FluentLaunch
{
    public class Item
    {
        public int Sort { get; set; }
        public BitmapSource Icon { get; set; }
        public string Title { get; set; }
        public string ToolTip { get; set; }
        public string Command { get; set; }
        public string Target { get; set; }

        public Item()
        {
        }

        public Item(int sort, BitmapSource ico, string title, string tip, string cmd, string target)
        {
            Sort = sort;
            Icon = ico;
            Title = title;
            ToolTip = tip;
            Command = cmd;
            Target = target;
        }
    }

    public class ItemStored
    {
        public int Sort { get; set; }
        public string Icon { get; set; }
        public string Title { get; set; }
        public string ToolTip { get; set; }
        public string Command { get; set; }
        public string Target { get; set; }

        public ItemStored()
        {
        }

        public ItemStored(int sort, string ico, string title, string tip, string cmd, string target)
        {
            Sort = sort;
            Icon = ico;
            Title = title;
            ToolTip = tip;
            Command = cmd;
            Target = target;
        }
    }
}