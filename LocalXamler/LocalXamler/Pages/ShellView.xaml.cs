using System;
using System.Collections.Generic;
using System.Data;
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

namespace LocalXamler.Pages
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        public ShellView()
        {
            InitializeComponent();
        }

        private void DataGrid_OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var vm = (ShellViewModel) this.DataContext;

            var datagrid = (DataGrid) sender;

            var editedTextbox = e.EditingElement as TextBox;

            if (editedTextbox == null )
            {
                return;
            }
            if ( (string.IsNullOrEmpty(editedTextbox.Text) || string.IsNullOrWhiteSpace(editedTextbox.Text)))
            {
                MessageBox.Show("必须输入!");
                e.Cancel = true;
                return;
            }

            if (e.Column.Header.ToString()=="Key")
            {
                if (preValue!=editedTextbox.Text)
                {
 //检查是否重复
               
                var keys = vm.Project.Datas.AsEnumerable().Select(r => r.Field<string>("Key")).ToList();
                var repeatkey = keys.FirstOrDefault(x => x == editedTextbox.Text);
                if (!(string.IsNullOrEmpty(repeatkey)||string.IsNullOrWhiteSpace(repeatkey)))
                {
                    MessageBox.Show("Key重复!");
                    e.Cancel = true;
                    return;

                }                    
                }
               
            }
        }

        private string preValue = "";
        private void DataGrid_OnBeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            preValue = (e.Column.GetCellContent(e.Row) as TextBlock)?.Text;  

        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("是否删除该项?", "删除?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }


            var item = (DataRowView)((Button)e.Source).DataContext;
            item.Row.Delete();
        }
    }
}
