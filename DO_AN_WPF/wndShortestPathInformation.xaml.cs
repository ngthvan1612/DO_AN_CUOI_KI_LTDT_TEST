using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for wndShortestPathInformation.xaml
    /// </summary>
    public partial class wndShortestPathInformation : Window
    {
        public string Result { get; set; } = "SDF";
        public Brush ResultForeColor { get; set; } = Brushes.Blue;
        public double TotalWeight { get; set; } = double.PositiveInfinity;
        public string ListVertex { get; set; } = "";
        public ObservableCollection<Edge> ListEdge { get; set; } = new ObservableCollection<Edge>();

        public wndShortestPathInformation()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}
