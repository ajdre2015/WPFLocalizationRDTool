using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Xml;
using Stylet;
using System.Linq;
namespace LocalXamler.Pages
{
    public class ShellViewModel : Screen
    {
        public string Name { get; set; } = "test";
        public void SayHello() => Name = "Hello " + Name;    // C#6的语法, 表达式方法
        public bool CanSayHello => !string.IsNullOrEmpty(Name);  // 同上
        public bool CanSave => Project?.LangInfos != null && Project.Datas != null;
        public void Save()
        {
            Project.Save();
        }
        public ProjectInfo Project { get; set; }
        public ShellViewModel()
        {
            Project = new ProjectInfo();
            Project.Init();
        }
    }

    public class ProjectInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public List<LangInfo> LangInfos { get; set; }
        public ObservableCollection<ExpandoObject> InputDatas { get; set; }
        public DataTable Datas { get; set; }
        public ProjectInfo()
        {
            Name = "test1";
            Path = "mydata.pjc";
            LangInfos = new List<LangInfo>()
            {
                new LangInfo(){Name = "Cn",Path = @"D:\work\赛默检测线\temp\Lang\cn.xaml"},
                new LangInfo(){Name = "En",Path = @"D:\work\赛默检测线\temp\Lang\En.xaml"},
            };
        }

        public void Init()
        {
            if (LangInfos==null)
            {
                return;
            }

            foreach (var info in LangInfos)
            {
                var myResourceDictionary = new ResourceDictionary();
                myResourceDictionary.Source =
                    new Uri(info.Path,
                        UriKind.RelativeOrAbsolute);
                info.Dictionary = myResourceDictionary;
            }
            InputDatas = new ObservableCollection<ExpandoObject>(GetInfo(LangInfos));
            var view = ToDataTable(InputDatas).DefaultView;
            view.Sort="Key";
            Datas = view.ToTable();
        }

        public void SetInfo(List<LangInfo> lang, DataTable dataTable)
        {
            var list = ToDynamic(dataTable);
            foreach (DataColumn column in dataTable.Columns)
            {
                if (column.ColumnName=="Key")
                {
                    continue;
                }
                var findlang = lang.FirstOrDefault(x => x.Name == column.ColumnName);
                if (findlang==null)
                {
                    findlang = new LangInfo(){Name = column.ColumnName,Path = $"{Name}.xaml"};
                    lang.Add(findlang);
                }
            }
            var keylist = new List<string>();
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (var info in lang)
                {
                        var key = row["Key"].ToString();
                        keylist.Add(key);
                        info.Dictionary[key] = row[info.Name];
                }
            }

            foreach (var info in lang)
            {
                var dickey = info.Dictionary.Keys.Cast<string>().ToList();
                var difflist = dickey.Except(keylist);
                foreach (var key in difflist)
                {
                    info.Dictionary.Remove(key);
                }
            }

            

        }

        public void Save()
        {
            SetInfo(LangInfos,Datas);
            Save(LangInfos);
        }
        private void Save(List<LangInfo> lang)
        {
            foreach (var info in lang)
            {
                Save(info);
            }
        }
        private void Save(LangInfo info)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter writer1 = XmlWriter.Create(info.Path, settings);
            XamlWriter.Save(info.Dictionary, writer1);
        }

        public List<ExpandoObject> GetInfo(List<LangInfo> lang)
        {
            var list = new List<ExpandoObject>();
            var keylist = new List<string>();
            foreach (var dic in lang)
            {
                foreach (var key in dic.Dictionary.Keys)
                {
                    var keystr = key.ToString();
                    var keyfind = keylist.FirstOrDefault(x => x == keystr);
                    if (keyfind==null)
                    {
                        keylist.Add(keystr);
                    }
                }
            }

            foreach (var key in keylist)
            {
                var expo = new ExpandoObject();
                var x = expo as IDictionary<string, Object>;
                x.Add("Key",key);
                foreach (var info in lang)
                {
                    x.Add(info.Name,info.Dictionary[key]);
                }
                list.Add(expo);
            }

            return list;
        }

        public DataTable ToDataTable(IEnumerable<ExpandoObject> items)
        {
            var data = items.ToArray();
            if (!data.Any()) return null;

            var dt = new DataTable();
            foreach (var key in ((IDictionary<string, object>)data[0]).Keys)
            {
                dt.Columns.Add(key);
            }
            foreach (var d in data)
            {
                dt.Rows.Add(((IDictionary<string, object>)d).Values.ToArray());
            }
            return dt;
        }

        public static List<ExpandoObject> ToDynamic(DataTable dt)
        {
            var dynamicDt = new List<ExpandoObject>();
            foreach (DataRow row in dt.Rows)
            {
                dynamic dyn = new ExpandoObject();
                dynamicDt.Add(dyn);
                foreach (DataColumn column in dt.Columns)
                {
                    var dic = (IDictionary<string, object>)dyn;
                    dic[column.ColumnName] = row[column];
                }
            }
            return dynamicDt;
        }
    }

    public class LangInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ResourceDictionary Dictionary { get; set; }

        
    }

    
}
