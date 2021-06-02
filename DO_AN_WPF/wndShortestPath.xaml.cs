using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DO_AN_WPF
{
    /// <summary>
    /// Interaction logic for wndShortestPath.xaml
    /// </summary>
    public partial class wndShortestPath : Window
    {
        private List<string> node;
        public string Source { get; private set; } = "";
        public string Target { get; private set; } = "";

        public wndShortestPath(List<int> listNode)
        {
            InitializeComponent();
            node = (from x in listNode
                   select x.ToString()).ToList();
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            string source = tbSource.Text;
            string target = tbTarget.Text;
            if (!node.Contains(source))
            {
                MessageBox.Show("Không có đỉnh nào tên '" + source + "'");
                return;
            }
            if (!node.Contains(target))
            {
                MessageBox.Show("Không có đỉnh nào tên '" + target + "'");
                return;
            }
            Source = source;
            Target = target;
            DialogResult = true;
        }
    }
}
