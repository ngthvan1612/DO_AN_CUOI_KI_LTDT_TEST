using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private ObservableCollection<Line> _lines;

        public ObservableCollection<Line> Lines
        {
            get
            {
                return _lines;
            }
            set
            {
                _lines = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Lines"));
            }
        }

        public GraphTheoryVisualizationControl()
        {
            InitializeComponent();

            this.mainDraw.PreviewMouseMove += MainDraw_PreviewMouseMove;
            List<Double> Dots = new List<double>();
            Dots.Add(1);
            Dots.Add(2);

            Lines = new ObservableCollection<Line>
            {
                new Line { From = new Point(100, 20), To = new Point(180, 180), Text = "a" },
                new Line { From = new Point(180, 180), To = new Point(20, 180), Text = "b" },
                new Line { From = new Point(20, 180), To = new Point(100, 20), Text = "c" },
                new Line { From = new Point(20, 50), To = new Point(180, 150) }
            };

            FirstPosition = new Rectangle();
            FirstPosition.Stroke = Brushes.DarkGray;
            FirstPosition.StrokeDashArray = new DoubleCollection(Dots);

            CurrentPosition = new Rectangle();
            CurrentPosition.Stroke = Brushes.DarkGray;
            CurrentPosition.StrokeDashArray = new DoubleCollection(Dots);

            mainDraw.DataContext = this;
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
                FirstPosition.Visibility = System.Windows.Visibility.Visible;
                CurrentPosition.Visibility = System.Windows.Visibility.Visible;

                double newLeft = e.GetPosition((MovingObject as FrameworkElement).Parent as FrameworkElement).X - FirstXPos;
                double newTop = e.GetPosition((MovingObject as FrameworkElement).Parent as FrameworkElement).Y - FirstYPos;

                for (int i = 0; i < 4; ++i)
                {
                    Node u = MovingObject as Node;
                    if (u.Content.ToString() == (i + 1).ToString())
                    {
                        Lines[i].To = new Point(newLeft + u.Width / 2, newTop + u.Height / 2);
                        status.Text = Lines[i].A.ToString() + "\n" + Lines[i].P.ToString();
                    }
                }

                newLeft = Math.Max(newLeft, 0);
                newLeft = Math.Min(newLeft, mainDraw.ActualWidth - 40);
                newTop = Math.Max(newTop, 0);
                newTop = Math.Min(newTop, mainDraw.ActualHeight - 40);

                (MovingObject as FrameworkElement).SetValue(Canvas.LeftProperty, newLeft);

                (MovingObject as FrameworkElement).SetValue(Canvas.TopProperty, newTop);
            }
        }

        private static int id = 1;

        public event PropertyChangedEventHandler PropertyChanged;

        private void btnAddNewNode_Click(object sender, RoutedEventArgs e)
        {
            AddNewVertex(id.ToString());
            id++;
        }

        private void AddNewVertex(string label)
        {
            Node vertex = new Node(label, mainDraw);
            vertex.PreviewMouseLeftButtonDown += Vertex_PreviewMouseLeftButtonDown;
            vertex.PreviewMouseLeftButtonUp += Vertex_PreviewMouseLeftButtonUp;
            vertex.Cursor = Cursors.Hand;
            mainDraw.Children.Add(vertex);
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
    }

    public class Node : Button
    {
        public Node(string label, Canvas parent) : base()
        {
            this.Content = label;
            this.Style = Application.Current.FindResource("RoundCorner") as Style;
        }
    }

    public class Line : INotifyPropertyChanged
    {
        private Point _from = new Point(0, 0);
        private Point _to = new Point(0, 0);
        private Point _p = new Point(0, 0);
        private Point _a = new Point(0, 0);
        private Point _b = new Point(0, 0);
        private Point _center = new Point(0, 0);
        private string _text = "(null)";

        public Point From
        {
            get { return _from; }
            set { _from = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("From")); }
        }

        
        public Point To
        {
            get { return _to; }
            set
            {
                _to = value;

                Point u = new Point(_to.X - From.X, _to.Y - From.Y);
                double length_u = Math.Sqrt(u.Y * u.Y + u.X * u.X);
                Point v = new Point(u.X / length_u, u.Y / length_u);
                Point PO = new Point(v.X * 20, v.Y * 20);
                Point p = new Point(Math.Round(To.X - PO.X, 2), Math.Round(To.Y - PO.Y, 2));
                P = p;

                double ang = Math.PI * 7 / 36;

                Point AP = new Point(v.X * Math.Cos(ang) - v.Y * Math.Sin(ang), v.X * Math.Sin(ang) + v.Y * Math.Cos(ang));
                A = new Point(P.X - AP.X  * 20, P.Y - AP.Y * 20);

                ang = -ang;
                Point BP = new Point(v.X * Math.Cos(ang) - v.Y * Math.Sin(ang), v.X * Math.Sin(ang) + v.Y * Math.Cos(ang));
                B = new Point(P.X - BP.X * 20, P.Y - BP.Y * 20);

                Center = new Point((From.X + _to.X) / 2, (From.Y + _to.Y) / 2);

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("To"));
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

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Text"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
