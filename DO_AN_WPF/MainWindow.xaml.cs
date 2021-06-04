﻿using System;
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
            totalWeight = 0.00;
            Random r = new Random(Guid.NewGuid().GetHashCode());
            List<int> vertices = graphLayout.GetListVertex();
            List<Edge> edges = graphLayout.GetListEdges();

            int n = vertices.Count;

            List<Edge>[] adj = new List<Edge>[n];
            for (int i = 0; i < n; ++i)
            {
                adj[i] = new List<Edge>();
            }

            for (int i = 0; i < edges.Count; ++i)
            {
                adj[edges[i].Source].Add(new Edge(edges[i].ID, edges[i].Source, edges[i].Target, edges[i].Weight));
            }

            bool[] inHeap = new bool[n];
            for (int i = 0; i < n; ++i)
            {
                inHeap[i] = false;
            }

            double[] distance = new double[n];
            for (int i = 0; i < n; ++i)
            {
                distance[i] = double.PositiveInfinity;
            }

            int[] parent = new int[n];
            for (int i = 0; i < n; ++i)
            {
                parent[i] = -1;
            }

            distance[source] = 0;

            while (true)
            {
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

                if (u >= 0)
                {
                    inHeap[u] = true;

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

            if (double.IsPositiveInfinity(distance[target]))
                return null;
            List<int> result = new List<int>();
            Stack<int> st = new Stack<int>();

            int temp = target;
            while (parent[temp] != -1)
            {
                st.Push(parent[temp]);
                temp = edges[parent[temp]].Source;
            }

            while (st.Count > 0)
            {
                result.Add(st.Pop());
            }

            totalWeight = distance[target];

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
