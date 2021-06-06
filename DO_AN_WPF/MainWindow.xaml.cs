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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace DO_AN_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void ClearAllHighlight()
        {
            graphLayout.ClearAllHighlight();
        }

        private List<int> Dijkstra(int source, int target, out double totalWeight)
        {
            //Lấy danh sách đỉnh ra
            List<int> vertices = graphLayout.GetListVertex();

            //Lấy danh sách cạnh ra
            List<Edge> edges = graphLayout.GetListEdges();

            //n là số đỉnh của đồ thị
            int n = vertices.Count;

            //inHeap[u] = true khi đỉnh u đã được tối ưu, ban đầu tất cả inHeap[u] = false
            bool[] inHeap = new bool[n];
            for (int i = 0; i < n; ++i)
            {
                inHeap[i] = false;
            }

            //distance[u] là độ dài đường đi ngắn nhất từ đỉnh source đến u
            //và distance[source] khởi tạo bằng vô cực
            double[] distance = new double[n];
            for (int i = 0; i < n; ++i)
            {
                distance[i] = double.PositiveInfinity;
            }
            distance[source] = 0;

            //parent[u] là đỉnh cha của u trên đồ thị đường đi ngắn nhất
            //ban đầu tất cả đều là -1 (không xác định)
            int[] parent = new int[n];
            for (int i = 0; i < n; ++i)
            {
                parent[i] = -1;
            }

            //kết quả độ dài đường đi ngắn nhất từ source đến target lưu ở biến này
            totalWeight = 0.00;

            //Tạo danh sách kề từ danh sách cạnh
            List<Edge>[] adj = new List<Edge>[n];
            for (int i = 0; i < n; ++i)
            {
                adj[i] = new List<Edge>();
            }

            for (int i = 0; i < edges.Count; ++i)
            {
                adj[edges[i].Source].Add(new Edge(edges[i].ID, edges[i].Source, edges[i].Target, edges[i].Weight));
            }

            //Dijkstra
            while (true)
            {
                //B1. Chọn đỉnh u mà u chưa tối ưu và có distance[u] nhỏ nhất
                //Nếu có nhiều đỉnh u như vậy thì chọn ra u có chỉ số nhỏ nhất
                //Vì ta thử i từ 0 đến n - 1, và chỉ cập nhật kết quả khi distance[u] > distance[i]
                //Giả sử u = -1 tức là không tìm thấy
                int u = -1;
                for (int i = 0; i < n; ++i)
                {
                    if (!inHeap[i])
                    {
                        if (u == -1)
                        {
                            u = i;
                        }
                        else
                        {
                            if (distance[u] > distance[i])
                            {
                                u = i;
                            }
                        }
                    }
                }

                //Nếu tìm thấy đỉnh u như vậy
                if (u >= 0)
                {
                    //Đánh dấu u đã được tối ưu
                    inHeap[u] = true;

                    //Cực tiểu hóa các distance[v] mà (u, v) thuộc E (tức là tồn tại cung (u, v))
                    foreach (Edge e in adj[u])
                    {
                        int v = e.Target;
                        if (!inHeap[v])
                        {
                            if (distance[v] > distance[u] + e.Weight)
                            {
                                distance[v] = distance[u] + e.Weight;
                                parent[v] = e.ID;
                            }
                        }
                    }
                }
                else break;
            }

            //Nếu distance[target] vẫn là vô cực, tức là không có đường đi => trả về null
            if (double.IsPositiveInfinity(distance[target]))
                return null;

            //Truy vết đường đi
            List<int> result = new List<int>(); //Lưu kết quả
            Stack<int> st = new Stack<int>(); //Truy vết từ đỉnh target về đỉnh source

            //Liên tục nhảy từ target lên parent[target], lên parent[parent[target]] cho đến khi nào không đi được nữa
            int temp = target;
            while (parent[temp] != -1)
            {
                st.Push(parent[temp]);
                temp = edges[parent[temp]].Source;
            }

            //Đảo ngược lại đường đi => cho danh sách cạnh vào kết quả
            while (st.Count > 0)
            {
                result.Add(st.Pop());
            }

            //Gán lại độ dài đường đi ngắn nhất
            totalWeight = distance[target];

            //Trả về kết quả
            return result;
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenGraph();
        }

        private void OpenGraph()
        {
            OpenFileDialog openDialog = new OpenFileDialog()
            {
                Filter = "Graph file|*.txt",
                Multiselect = false
            };
            if (openDialog.ShowDialog() == true)
            {
                if (!graphLayout.LoadFromFile(openDialog.FileName))
                {
                    MessageBox.Show("File lỗi");
                }
            }
        }

        private void FindShortestPathAndHighlight()
        {
            wndShortestPath frm = new wndShortestPath(graphLayout.GetListVertex());
            frm.Owner = this;
            if (frm.ShowDialog() == true)
            {
                ClearAllHighlight();
                var listEdgeId = Dijkstra(Convert.ToInt32(frm.Source), Convert.ToInt32(frm.Target), out double totalWeight);
                wndShortestPathInformation info = new wndShortestPathInformation();
                if (listEdgeId == null)
                {
                    info.Result = "Không tìm thấy đường đi";
                    info.ResultForeColor = Brushes.Red;
                }
                else
                {
                    graphLayout.Highlight(listEdgeId, Brushes.Red);
                    info.Result = "Đã tìm thấy đường đi";
                    info.ResultForeColor = Brushes.Green;
                    info.TotalWeight = totalWeight;
                    StringBuilder sb = new StringBuilder();
                    var listEdge = graphLayout.GetListEdges();
                    info.ListEdge.Clear();
                    if (frm.Source == frm.Target && listEdgeId.Count == 0)
                    {
                        sb.Append(frm.Source);
                        info.TotalWeight = 0;
                    }
                    for (int i = 0; i < listEdgeId.Count; ++i)
                    {
                        sb.Append(listEdge[listEdgeId[i]].Source);
                        sb.Append("\u279D");
                        if (i == listEdgeId.Count - 1)
                        {
                            sb.Append(listEdge[listEdgeId[i]].Target);
                        }
                        info.ListEdge.Add(listEdge[listEdgeId[i]]);
                    }
                    info.ListVertex = sb.ToString();
                }
                info.Owner = this;
                info.ShowDialog();
            }
        }

        private void btnRedraw_Click(object sender, RoutedEventArgs e)
        {
            graphLayout.ReLoad();
        }

        private void btnFindShortestPath_Click(object sender, RoutedEventArgs e)
        {
            FindShortestPathAndHighlight();
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            wndInfo frm = new wndInfo()
            {
                Owner = this
            };
            frm.ShowDialog();
        }
    }
}
