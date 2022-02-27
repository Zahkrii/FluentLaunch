using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FluentLaunch
{
    internal class XMLProcess
    {
        private static string _path = Directory.GetCurrentDirectory() + "\\Assets";

        private static XmlDocument xml = new XmlDocument();

        public static IList<Item> LoadLayout(string category)
        {
            IList<Item> _items = new ObservableCollection<Item>();
            xml.Load(_path + "\\layout.xml");
            XmlNodeList nodeList = xml.SelectSingleNode("Layout").ChildNodes;//获取Layout节点的所有子节点
            foreach (XmlNode xn in nodeList)//遍历所有子节点
            {
                XmlElement xe = (XmlElement)xn;//将子节点类型转换为XmlElement类型
                if (xe.GetAttribute("Category") == category)
                {
                    XmlNodeList nls = xe.ChildNodes;//继续获取xe子节点的所有子节点items
                    foreach (XmlNode xn1 in nls)//遍历items
                    {
                        XmlElement xe2 = (XmlElement)xn1;//转换类型
                        int sort = -1;
                        int.TryParse(xe2.GetAttribute("Sort"), out sort);
                        _items.Add(new Item(sort,
                            ImageProcess.CreateBitmapSourceFromGdiBitmap(new Bitmap(xe2.SelectSingleNode("Icon").InnerText)),
                            xe2.SelectSingleNode("Title").InnerText, xe2.SelectSingleNode("ToolTip").InnerText,
                            xe2.SelectSingleNode("Command").InnerText, xe2.SelectSingleNode("Target").InnerText));
                    }
                }
            }
            _items = new ObservableCollection<Item>(_items.OrderBy(i => i.Sort));
            return _items;
        }

        public static void AddLayoutItem(ItemStored item, Category category)
        {
            XmlElement xmlItem = xml.CreateElement("Item");

            XmlElement xmlItemSub = xml.CreateElement("Icon");
            xmlItemSub.InnerText = item.Icon;
            xmlItem.AppendChild(xmlItemSub);

            xmlItemSub = xml.CreateElement("Title");
            xmlItemSub.InnerText = item.Title;
            xmlItem.AppendChild(xmlItemSub);

            xmlItemSub = xml.CreateElement("ToolTip");
            xmlItemSub.InnerText = item.ToolTip;
            xmlItem.AppendChild(xmlItemSub);

            xmlItemSub = xml.CreateElement("Command");
            xmlItemSub.InnerText = item.Command;
            xmlItem.AppendChild(xmlItemSub);

            xmlItemSub = xml.CreateElement("Target");
            xmlItemSub.InnerText = item.Target;
            xmlItem.AppendChild(xmlItemSub);

            xml.Load(_path + "\\layout.xml");

            XmlNodeList layoutNodes = xml.SelectSingleNode("Layout").ChildNodes;
            foreach (XmlNode xn in layoutNodes)//遍历所有子节点
            {
                XmlElement xe = (XmlElement)xn;//将子节点类型转换为XmlElement类型
                if (xe.GetAttribute("Category") == category.Name)
                {
                    XmlNodeList nls = xe.ChildNodes;//继续获取xe子节点的所有子节点items
                    string sort = (nls.Count).ToString();
                    xmlItem.SetAttribute("Sort", sort);
                    xe.AppendChild(xmlItem);
                }
            }
            xml.Save(_path + "\\layout.xml");
        }

        /// <summary>
        /// 更新 LayoutItem
        /// </summary>
        /// <param name="updatedItem">新的item</param>
        /// <param name="category">item所在分类</param>
        public static void UpdateLayoutItem(ItemStored updatedItem, Category category)
        {
            DeleteLayoutItem(updatedItem.Sort, category);

            XmlElement xmlItem = xml.CreateElement("Item");

            XmlElement xmlItemSub = xml.CreateElement("Icon");
            xmlItemSub.InnerText = updatedItem.Icon;
            xmlItem.AppendChild(xmlItemSub);

            xmlItemSub = xml.CreateElement("Title");
            xmlItemSub.InnerText = updatedItem.Title;
            xmlItem.AppendChild(xmlItemSub);

            xmlItemSub = xml.CreateElement("ToolTip");
            xmlItemSub.InnerText = updatedItem.ToolTip;
            xmlItem.AppendChild(xmlItemSub);

            xmlItemSub = xml.CreateElement("Command");
            xmlItemSub.InnerText = updatedItem.Command;
            xmlItem.AppendChild(xmlItemSub);

            xmlItemSub = xml.CreateElement("Target");
            xmlItemSub.InnerText = updatedItem.Target;
            xmlItem.AppendChild(xmlItemSub);

            xmlItem.SetAttribute("Sort", updatedItem.Sort.ToString());

            xml.Load(_path + "\\layout.xml");

            XmlNodeList layoutNodes = xml.SelectSingleNode("Layout").ChildNodes;
            foreach (XmlNode xn in layoutNodes)//遍历所有子节点
            {
                XmlElement xe = (XmlElement)xn;//将子节点类型转换为XmlElement类型
                if (xe.GetAttribute("Category") == category.Name)
                {
                    xe.AppendChild(xmlItem);
                }
            }
            xml.Save(_path + "\\layout.xml");
        }

        /// <summary>
        /// 删除item
        /// </summary>
        /// <param name="deletedItem">将要删除的item</param>
        /// <param name="category">item所在分类</param>
        public static void DeleteLayoutItem(int itemSort, Category category)
        {
            xml.Load(_path + "\\layout.xml");
            XmlNodeList nodeList = xml.SelectSingleNode("Layout").ChildNodes;//获取Layout节点的所有子节点
            foreach (XmlNode xn in nodeList)//遍历所有子节点
            {
                XmlElement xe = (XmlElement)xn;//将子节点类型转换为XmlElement类型
                if (xe.GetAttribute("Category") == category.Name)
                {
                    XmlNodeList nls = xe.ChildNodes;//继续获取xe子节点的所有子节点items
                    foreach (XmlNode xn1 in nls)//遍历items
                    {
                        XmlElement xe2 = (XmlElement)xn1;//转换类型
                        int sort;
                        int.TryParse(xe2.GetAttribute("Sort"), out sort);
                        if (sort == itemSort)
                        {
                            xe.RemoveChild(xe2);
                        }
                    }
                }
            }
            xml.Save(_path + "\\layout.xml");
        }

        /// <summary>
        /// 载入所有分类
        /// </summary>
        /// <returns>分类排序后的列表</returns>
        public static IList<Category> LoadCategory()
        {
            IList<Category> _categorys = new ObservableCollection<Category>();
            xml.Load(_path + "\\category.xml");
            XmlNodeList nodeList = xml.SelectSingleNode("Categorys").ChildNodes;//获取Categorys节点的所有子节点
            foreach (XmlNode xn in nodeList)//遍历所有子节点
            {
                XmlElement xe = (XmlElement)xn;//将子节点类型转换为XmlElement类型
                int id;
                int.TryParse(xe.GetAttribute("Id"), out id);
                _categorys.Add(new Category(id, xe.GetAttribute("Name")));
            }
            _categorys = new ObservableCollection<Category>(_categorys.OrderBy(i => i.Id));
            return _categorys;
        }

        public static void ResortXML()
        {
            xml.Load(_path + "\\layout.xml");
            XmlNodeList nodeList = xml.SelectSingleNode("Layout").ChildNodes;//获取Layout节点的所有子节点
            for (int j = 0; j < nodeList.Count; j++)//遍历所有子节点
            {
                IList<ItemStored> _items = new ObservableCollection<ItemStored>();
                XmlNode sectionNode = nodeList.Item(j);
                XmlElement sectionElement = (XmlElement)sectionNode;//将子节点类型转换为XmlElement类型
                XmlNodeList itemNodeList = sectionElement.ChildNodes;//继续获取xe子节点的所有子节点items
                for (int i = 0; i < itemNodeList.Count; i++)//遍历items
                {
                    XmlNode xn1 = itemNodeList.Item(i);
                    XmlElement xe2 = (XmlElement)xn1;//转换类型
                    int sort;
                    int.TryParse(xe2.GetAttribute("Sort"), out sort);
                    _items.Add(new ItemStored(sort, xe2.SelectSingleNode("Icon").InnerText,
                        xe2.SelectSingleNode("Title").InnerText, xe2.SelectSingleNode("ToolTip").InnerText,
                        xe2.SelectSingleNode("Command").InnerText, xe2.SelectSingleNode("Target").InnerText));
                }
                sectionElement.InnerXml = null;
                _items = new ObservableCollection<ItemStored>(_items.OrderBy(i => i.Sort));
                int k = 0;
                foreach (ItemStored itemStored in _items)
                {
                    XmlElement xmlItem = xml.CreateElement("Item");

                    XmlElement xmlItemSub = xml.CreateElement("Icon");
                    xmlItemSub.InnerText = itemStored.Icon;
                    xmlItem.AppendChild(xmlItemSub);

                    xmlItemSub = xml.CreateElement("Title");
                    xmlItemSub.InnerText = itemStored.Title;
                    xmlItem.AppendChild(xmlItemSub);

                    xmlItemSub = xml.CreateElement("ToolTip");
                    xmlItemSub.InnerText = itemStored.ToolTip;
                    xmlItem.AppendChild(xmlItemSub);

                    xmlItemSub = xml.CreateElement("Command");
                    xmlItemSub.InnerText = itemStored.Command;
                    xmlItem.AppendChild(xmlItemSub);

                    xmlItemSub = xml.CreateElement("Target");
                    xmlItemSub.InnerText = itemStored.Target;
                    xmlItem.AppendChild(xmlItemSub);

                    xmlItem.SetAttribute("Sort", k.ToString());

                    sectionElement.AppendChild(xmlItem);
                    k++;
                }
            }
            xml.Save(_path + "\\layout.xml");
        }
    }
}