using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
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

namespace DO_AN_WPF
{
    public partial class GraphTheoryVisualizationControl : UserControl, INotifyPropertyChanged
    {
        private double FirstXPos, FirstYPos, FirstArrowXPos, FirstArrowYPos;
        private object MovingObject;
        private Rectangle FirstPosition, CurrentPosition;
        private ObservableCollection<Edge> _edges;

        private string currentFile = "";

        private SortedDictionary<int, Vertex> Vertices { get; set; } = new SortedDictionary<int, Vertex>();

        public ObservableCollection<Edge> Edges
        {
            get
            {
                return _edges;
            }
            set
            {
                _edges = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Edges"));
            }
        }

        public GraphTheoryVisualizationControl()
        {
            InitializeComponent();

            this.mainDraw.PreviewMouseMove += MainDraw_PreviewMouseMove;
            List<Double> Dots = new List<double>();
            Dots.Add(1);
            Dots.Add(2);

            Edges = new ObservableCollection<Edge>();

            FirstPosition = new Rectangle();
            FirstPosition.Stroke = Brushes.DarkGray;
            FirstPosition.StrokeDashArray = new DoubleCollection(Dots);

            CurrentPosition = new Rectangle();
            CurrentPosition.Stroke = Brushes.DarkGray;
            CurrentPosition.StrokeDashArray = new DoubleCollection(Dots);

            mainDraw.DataContext = this;
        }

        public List<int> GetListVertex()
        {
            return Vertices.Keys.ToList();
        }

        public List<Edge> GetListEdges()
        {
            return Edges.ToList();
        }

        public void ReLoad()
        {
            try
            {
                LoadFromFile(currentFile);
            }
            catch
            {
                MessageBox.Show("File lỗi");
            }
        }

        private void MainDraw_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && MovingObject is FrameworkElement)
            {
                FirstPosition.Width = (MovingObject as FrameworkElement).ActualWidth;
                FirstPosition.Height = (MovingObject as FrameworkElement).ActualHeight;
                FirstPosition.SetValue(Canvas.LeftProperty, FirstArrowXPos);
                FirstPosition.SetValue(Canvas.TopProperty, FirstArrowYPos);

                CurrentPosition.Width = (MovingObject as FrameworkElement).ActualWidth;
                CurrentPosition.Height = (MovingObject as FrameworkElement).ActualHeight;
                FirstPosition.Visibility = Visibility.Visible;
                CurrentPosition.Visibility = Visibility.Visible;

                double newLeft = e.GetPosition((MovingObject as FrameworkElement).Parent as FrameworkElement).X - FirstXPos;
                double newTop = e.GetPosition((MovingObject as FrameworkElement).Parent as FrameworkElement).Y - FirstYPos;

                newLeft = Math.Max(newLeft, 0);
                newLeft = Math.Min(newLeft, mainDraw.ActualWidth - 40);
                newTop = Math.Max(newTop, 0);
                newTop = Math.Min(newTop, mainDraw.ActualHeight - 40);

                Vertex u = MovingObject as Vertex;

                foreach (int edge_id in u.ListEdge)
                {
                    if (Edges[edge_id].Source.ToString() == u.Text)
                    {
                        Edges[edge_id].From = new Point(newLeft + u.Width / 2, newTop + u.Height / 2);
                    }
                    else
                    {
                        Edges[edge_id].To = new Point(newLeft + u.Width / 2, newTop + u.Height / 2);
                    }
                }

                (MovingObject as FrameworkElement).SetValue(Canvas.LeftProperty, newLeft);

                (MovingObject as FrameworkElement).SetValue(Canvas.TopProperty, newTop);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ClearAll()
        {
            Edges.Clear();
            foreach (var p in Vertices)
            {
                mainDraw.Children.Remove(p.Value);
            }
            Vertices.Clear();
        }

        public void ClearAllHighlight()
        {
            foreach (var e in Edges)
            {
                e.EdgeColor = Brushes.Black;
            }
        }

        public void Highlight(List<int> listEdgeId, Brush color)
        {
            foreach (int v in listEdgeId)
            {
                Edges[v].EdgeColor = color;
            }
        }

        private void AddNewVertex(int label, int top = 0, int left = 0)
        {
            Vertex vertex = new Vertex(label, mainDraw);
            vertex.PreviewMouseLeftButtonDown += Vertex_PreviewMouseLeftButtonDown;
            vertex.PreviewMouseLeftButtonUp += Vertex_PreviewMouseLeftButtonUp;
            vertex.Cursor = Cursors.Hand;
            mainDraw.Children.Add(vertex);
            vertex.SetValue(Canvas.LeftProperty, 1.0 * left);
            vertex.SetValue(Canvas.TopProperty, 1.0 * top);
            Vertices.Add(label, vertex);
        }

        private void AddNewEdge(int source, int target, double weight)
        {
            Edge line = new Edge();
            line.From = new Point(Canvas.GetLeft(Vertices[source]) + 20, Canvas.GetTop(Vertices[source]) + 20);
            line.To = new Point(Canvas.GetLeft(Vertices[target]) + 20, Canvas.GetTop(Vertices[target]) + 20);
            line.Source = source;
            line.Target = target;
            line.Weight = weight;
            line.ID = Edges.Count;
            Edges.Add(line);
            int id = Edges.Count - 1;
            Vertices[source].ListEdge.Add(id);
            Vertices[target].ListEdge.Add(id);
        }

        private void Line_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void Vertex_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MovingObject = null;
            FirstPosition.Visibility = Visibility.Hidden;
            CurrentPosition.Visibility = Visibility.Hidden;
        }

        private void Vertex_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FirstXPos = e.GetPosition(sender as Control).X;
            FirstYPos = e.GetPosition(sender as Control).Y;
            FirstArrowXPos = e.GetPosition((sender as Control).Parent as Control).X - FirstXPos;
            FirstArrowYPos = e.GetPosition((sender as Control).Parent as Control).Y - FirstYPos;
            MovingObject = sender;
        }

        public bool LoadFromFile(string fileName)
        {
            ClearAll();
            using (StreamReader sr = new StreamReader(new FileStream(fileName, FileMode.Open)))
            {
                try
                {
                    var tmp_nm = (from x in sr.ReadLine().Split(new char[] { ' ' }).ToList()
                                      where x.Length > 0
                                      select Convert.ToInt32(x)).ToList();
                    int n = tmp_nm[0], m = tmp_nm[1];
                    Random random = new Random(Guid.NewGuid().GetHashCode());
                    for (int i = 0; i < n; ++i)
                    {
                        int top = random.Next() % ((int)mainDraw.ActualHeight - 40);
                        int left = random.Next() % ((int)mainDraw.ActualWidth - 40);
                        AddNewVertex(i, top, left);
                    }
                    for (int i = 0; i < m; ++i)
                    {
                        var tmp_uv = (from x in sr.ReadLine().Split(new char[] { ' ' }).ToList()
                                      where x.Length > 0
                                      select Convert.ToDouble(x)).ToList();
                        if (tmp_uv.Count == 3)
                        {
                            int u = (int)tmp_uv[0], v = (int)tmp_uv[1];
                            double weight = tmp_uv[2];
                            AddNewEdge(u, v, weight);
                        }
                    }
                    currentFile = fileName;
                }
                catch
                {
                    ClearAll();
                    return false;
                }
                return true;
            }
        }
    }

    public class Vertex : Button
    {
        public SortedSet<int> ListEdge { get; set; } = new SortedSet<int>();

        public string Text { get => this.Content.ToString(); }

        public Vertex(int label, Canvas parent) : base()
        {
            this.Content = label.ToString();
            this.Style = Application.Current.FindResource("RoundCorner") as Style;
        }
    }

    public class Edge : INotifyPropertyChanged
    {
        private static double ARROW_LENGTH = 17.0;

        public int Source { get; set; } = -1;
        public int Target { get; set; } = -1;

        private int _id = 0;
        private double _textAngle = 0.0;
        private Point _from = new Point(0, 0);
        private Point _to = new Point(0, 0);
        private Point _p = new Point(0, 0);
        private Point _a = new Point(0, 0);
        private Point _b = new Point(0, 0);
        private Point _center = new Point(0, 0);
        private PointCollection _headArrow = new PointCollection(new Point[] { new Point(100, 0), new Point(0, 0), new Point(0, 0) });
        private Brush _lineColor = Brushes.Black;
        private double _weight = 0.0;

        public Edge() { }

        public Edge(int id, int source, int target, double weight)
        {
            ID = id;
            Source = source;
            Target = target;
            Weight = weight;
        }

        public Brush EdgeColor
        {
            get
            {
                return _lineColor;
            }
            set
            {
                _lineColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EdgeColor"));
            }
        }

        private void RedrawLine()
        {
            Center = new Point((From.X + To.X) / 2, (From.Y + To.Y) / 2);

            Point u = new Point(To.X - From.X, To.Y - From.Y);
            double length_u = Math.Sqrt(u.Y * u.Y + u.X * u.X);
            Point v = new Point(u.X / length_u, u.Y / length_u);
            Point PO = new Point(v.X * 0, v.Y * 0);
            P = new Point(Math.Round(Center.X - PO.X, 2), Math.Round(Center.Y - PO.Y, 2));

            double ang = Math.PI * 35 / 180;

            Point AP = new Point(v.X * Math.Cos(ang) - v.Y * Math.Sin(ang), v.X * Math.Sin(ang) + v.Y * Math.Cos(ang));
            A = new Point(P.X - AP.X * ARROW_LENGTH, P.Y - AP.Y * ARROW_LENGTH);

            ang = -ang;
            Point BP = new Point(v.X * Math.Cos(ang) - v.Y * Math.Sin(ang), v.X * Math.Sin(ang) + v.Y * Math.Cos(ang));
            B = new Point(P.X - BP.X * ARROW_LENGTH, P.Y - BP.Y * ARROW_LENGTH);

            TextAngle = 180.0 * Math.Atan(u.Y / u.X) / Math.PI;
        }

        public int ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ID"));
            }
        }

        public Point From
        {
            get { return _from; }
            set
            {
                _from = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("From"));
                RedrawLine();
            }
        }
        
        public Point To
        {
            get { return _to; }
            set
            {
                _to = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("To"));
                RedrawLine();
            }
        }

        public Point P
        {
            get
            {
                return _p;
            }
            set
            {
                _p = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("P"));
            }
        }

        public Point A
        {
            get
            {
                return _a;
            }
            set
            {
                _a = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("A"));
            }
        }

        public Point B
        {
            get
            {
                return _b;
            }
            set
            {
                _b = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("B"));
            }
        }

        public Point Center
        {
            get
            {
                return _center;
            }
            set
            {
                _center = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Center"));
            }
        }

        public double Weight
        {
            get
            {
                return _weight;
            }
            set
            {
                _weight = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Weight"));
            }
        }

        public double TextAngle
        {
            get
            {
                return _textAngle;
            }
            set
            {
                _textAngle = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextAngle"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
