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
using System.Windows.Media.Animation;

namespace bub
{
    /// <summary>
    /// Interaction logic for winMain.xaml
    /// </summary>
    public partial class winMain : Window
    {
        private static winMain _inst;

        public static winMain Inst
        {
            get
            {
                return _inst;
            }
        }

        public winMain()
        {
            _inst = this;
            InitializeComponent();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Bubbler.NewGame();
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            Bubbler.Undo();
        }
    }

    public class Bubble : FrameworkElement
    {
        private Color _color;
        private Point _pos;

        private Bubbler _Parent;

        public Bubble(Bubbler Parent, Point pos, Color color)
        {
            _pos = pos;
            _color = color;
            _Parent = Parent;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
        }
    }

    public class Bubbler : FrameworkElement
    {
        private Array _bubbles;
        private Color[] _colors;
        private List<Ball> _selection;
        private List<Ball> _LastSelection;

        public static readonly DependencyProperty AnimationValueProperty =
            DependencyProperty.RegisterAttached("AnimationValue", typeof(double), typeof(Bubbler),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double AnimationValue
        {
            get
            {
                return (double) GetValue(AnimationValueProperty);
            }
            set
            {
                SetValue(AnimationValueProperty, value);
            }
        }

        private List<Ball> Selection
        {
            get
            {
                return _selection;
            }
            set
            {
                if (null != _selection)
                    _LastSelection = _selection;
                _selection = value;
            }
        }

        public const int NumberOfColors = 5; // 5
        public const int CellsY = 16; // 16
        public const int CellsX = 11; // 11
        public const double CellSize = 20; // 20
        public const double BubbleRadius = 8; // 8
        public const double ShakeRadius = BubbleRadius / 4.0;
        public const double ShakeFrom = -0.3;
        public const double ShakeTo = 0.3;
        public const double ShakePeriod = 0.5;

        public static readonly DependencyProperty ScoreProperty = DependencyProperty.RegisterAttached("Score", typeof(int), typeof(Bubbler));
        public static readonly DependencyProperty TurnCountProperty = DependencyProperty.RegisterAttached("TurnCount", typeof(int), typeof(Bubbler));

        private int _lastScore;

        public int Score
        {
            get
            {
                return (int) GetValue(ScoreProperty);
            }
            set
            {
                _lastScore = Score;
                SetValue(ScoreProperty, value);
            }
        }

        private int _lastTurnCount;

        public int TurnCount
        {
            get
            {
                return (int)GetValue(TurnCountProperty);
            }
            set
            {
                _lastTurnCount = TurnCount;
                SetValue(TurnCountProperty, value);
            }
        }

        public class BallStats : IComparable
        {
            private int m_Count;
            private Color m_Color;

            public int CompareTo(object obj)
            {
                BallStats b = obj as BallStats;
                if (null == b)
                    return 0;
                return b.m_Count-m_Count;
            }

            public BallStats(Color Color)
            {
                m_Count = 0;
                m_Color = Color;
            }

            public int Count
            {
                get
                {
                    return m_Count;
                }
                set
                {
                    m_Count = value;
                }
            }

            public SolidColorBrush Color
            {
                get
                {
                    return new SolidColorBrush(m_Color);
                }
            }

            public void Increment()
            {
                m_Count++;
            }
        }

        public static readonly DependencyProperty BallStatisticsProperty = DependencyProperty.RegisterAttached("BallStatistics", typeof(List<BallStats>), typeof(Bubbler));

        public List<BallStats> BallStatistics
        {
            get
            {
                return (List<BallStats>)GetValue(BallStatisticsProperty);
            }
            set
            {
                SetValue(BallStatisticsProperty, value);
            }
        }

        public static readonly DependencyProperty SelectionPointsProperty = DependencyProperty.RegisterAttached("SelectionPoints", typeof(int), typeof(Bubbler));
        public static readonly DependencyProperty SelectionCountProperty = DependencyProperty.RegisterAttached("SelectionCount", typeof(int), typeof(Bubbler));

        private int _lastSelPoints;

        public int SelectionPoints
        {
            get
            {
                return (int) GetValue(SelectionPointsProperty);
            }
            set
            {
                _lastSelPoints = SelectionPoints;
                SetValue(SelectionPointsProperty, value);
            }
        }

        private int _lastSelCount;

        public int SelectionCount
        {
            get
            {
                return (int)GetValue(SelectionCountProperty);
            }
            set
            {
                _lastSelCount = SelectionCount;
                SetValue(SelectionCountProperty, value);
            }
        }

        private void UpdateStatistics()
        {
            List<BallStats> stats = new List<BallStats>(NumberOfColors);
            for (int i = 0; i < NumberOfColors; i++)
            {
                stats.Add(new BallStats(_colors[i+1]));
            }
            for (int y = 0; y < CellsY; y++)
            {
                for (int x = 0; x < CellsX; x++)
                {
                    int color = (int) _bubbles.GetValue(y, x);
                    if (0 == color)
                        continue;
                    stats[color - 1].Increment();
                }
            }
            stats.Sort();
            BallStatistics = stats;
        }

        public Bubbler()
        {
            _bubbles = Array.CreateInstance(typeof(int),CellsY,CellsX);
            _colors = new Color[NumberOfColors+1];

            _colors[0] = Colors.Black;

            for (int i = 1; i <= NumberOfColors; i++)
            {
                int r, g, b;
                HSVColorConverter.HsvToRgb(360.0 * i / NumberOfColors, 1, 1, out r, out g, out b);
                _colors[i] = Color.FromRgb((byte) r, (byte) g, (byte) b);
            }

            OnGameOverEvent = OnGameOver;

            Generate();
            Score = 0;

            DoubleAnimation DA = new DoubleAnimation(ShakeFrom, ShakeTo, TimeSpan.FromSeconds(ShakePeriod));
            DA.RepeatBehavior = RepeatBehavior.Forever;
            DA.AutoReverse = true;
            BeginAnimation(AnimationValueProperty, DA);
        }

        public void NewGame()
        {
            Generate();
            Score = 0;
            SelectionCount = 0;
            SelectionPoints = 0;
            Selection = null;
            _lastField = null;
            TurnCount = 0;
            CheckUndo();
            InvalidateVisual();
        }

        private Array _lastField;

        public bool CheckGameOver()
        {
            for (int y = 0; y < CellsY-1; y++)
            {
                for (int x = 0; x < CellsX-1; x++)
                {
                    int color = (int) _bubbles.GetValue(y, x);
                    if (0 == color)
                        continue;
                    int color1 = (int)_bubbles.GetValue(y+1, x);
                    int color2 = (int)_bubbles.GetValue(y, x+1);
                    if (color1 == color || color2 == color)
                        return false;
                }
            }
            for (int y = 0; y < CellsY-1; y++)
            {
                int color = (int)_bubbles.GetValue(y, CellsX - 1);
                if (0 == color)
                    continue;
                int color1 = (int)_bubbles.GetValue(y + 1, CellsX - 1);
                if (color1 == color)
                    return false;
            }
            for (int x = 0; x < CellsX - 1; x++)
            {
                int color = (int)_bubbles.GetValue(CellsY - 1, x);
                if (0 == color)
                    continue;
                int color1 = (int)_bubbles.GetValue(CellsY - 1, x + 1);
                if (color1 == color)
                    return false;
            }
            return true;
        }

        private void Generate()
        {
            Random rnd = new Random();
            for (int y = 0; y < CellsY; y++)
            {
                for (int x = 0; x < CellsX; x++)
                {
                    _bubbles.SetValue(rnd.Next(1, NumberOfColors+1), y, x);
                }
            }
            UpdateStatistics();
        }

        public bool IsSelected(int x, int y)
        {
            if (null == Selection)
                return false;
            foreach (Ball ball in Selection)
            {
                if (ball.X == x && ball.Y == y)
                    return true;
            }
            return false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Transform tr = RenderTransform;
            Brush brush = new SolidColorBrush(Colors.LightGray);
            drawingContext.DrawRectangle(brush, null, tr.TransformBounds(new Rect(0, 0, CellsX * CellSize, CellsY * CellSize)));
            for (int y = 0; y < CellsY; y++)
            {
                for (int x = 0; x < CellsX; x++)
                {
                    int color = (int) _bubbles.GetValue(y, x);
                    if (0 == color)
                        continue;
                    Color borderColor = Color.Multiply(_colors[color], 0.2f);
                    borderColor.A = 224;
                    Brush br = new RadialGradientBrush(_colors[color], borderColor);

                    bool isSelected = IsSelected(x, y);

                    drawingContext.DrawEllipse(br, new Pen(br, 0.0),
                                                tr.Transform(new Point(x * CellSize + CellSize / 2.0, y * CellSize + CellSize / 2.0)),
                                                BubbleRadius + (isSelected ? AnimationValue * ShakeRadius : 0.0), 
                                                BubbleRadius - (isSelected ? AnimationValue * ShakeRadius : 0.0));
                }
            }
            if (null != Selection)
            {
                Color c = Color.FromArgb(128, 32, 64, 128);
                Brush b = new SolidColorBrush(c);
                Pen p = new Pen(b, 0.0);
                foreach (Ball ball in Selection)
                {
                    drawingContext.DrawRectangle(b, p, new Rect(ball.X * CellSize, ball.Y * CellSize, CellSize, CellSize));
                }
            }
        }

        private struct Ball
        {
            public int X;
            public int Y;

            public Ball(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        private void SelectRecursive(List<Ball> selection, Array mask, int px, int py, int color)
        {
            if ((bool) mask.GetValue(py, px))
                return;
            if ((int)_bubbles.GetValue(py, px) != color)
                return;
            mask.SetValue(true, py, px);
            selection.Add(new Ball(px, py));
            if (px - 1 >= 0)
                SelectRecursive(selection, mask, px - 1, py, color);
            if (px + 1 < CellsX)
                SelectRecursive(selection, mask, px + 1, py, color);
            if (py - 1 >= 0)
                SelectRecursive(selection, mask, px, py - 1, color);
            if (py + 1 < CellsY)
                SelectRecursive(selection, mask, px, py + 1, color);
        }

        private List<Ball> GetSelection(int px, int py)
        {
            List<Ball> selection = new List<Ball>();
            Array mask = Array.CreateInstance(typeof(bool), CellsY, CellsX);
            int x, y;
            for (y = 0; y < CellsY; y++)
            {
                for (x = 0; x < CellsX; x++)
                {
                    mask.SetValue(false, y, x);
                }
            }

            SelectRecursive(selection, mask, px, py, (int) _bubbles.GetValue(py, px));

            return selection;
        }

        public void Undo()
        {
            _bubbles = Array.CreateInstance(typeof(int), CellsY, CellsX);
            Array.Copy(_lastField, _bubbles, _bubbles.Length); 
            Score = _lastScore;
            Selection = _LastSelection;
            SelectionCount = _lastSelCount;
            SelectionPoints = _lastSelPoints;
            TurnCount = _lastTurnCount;
            UpdateStatistics();
            _lastField = null;
            CheckUndo();
            InvalidateVisual();
        }

        public static readonly DependencyProperty CanUndoProperty = DependencyProperty.RegisterAttached("CanUndo", typeof(bool), typeof(Bubbler));

        public bool CanUndo
        {
            get
            {
                return (bool) GetValue(CanUndoProperty);
            }
            protected set
            {
                SetValue(CanUndoProperty, value);
            }
        }

        private void CheckUndo()
        {
            CanUndo = (null != _lastField);
        }

        private void MoveAll()
        {
            Array newbub = Array.CreateInstance(typeof(int), CellsY, CellsX);

            int x, y;
            for (x = 0; x < CellsX; x++)
            {
                for (y = 0; y < CellsY; y++)
                {
                    newbub.SetValue(0, y, x);
                }
            }

            for (x = 0; x < CellsX; x++)
            {
                int y1 = CellsY - 1;
                for (y = CellsY-1; y >= 0; y--)
                {
                    int color = (int) _bubbles.GetValue(y,x);
                    if (0 != color)
                    {
                        newbub.SetValue(color, y1, x);
                        y1--;
                    }
                }
                if (CellsY - 1 == y1)
                {
                    for (int x1 = x; x1 > 0; x1--)
                    {
                        for (y1 = 0; y1 < CellsY; y1++)
                        {
                            newbub.SetValue(newbub.GetValue(y1, x1 - 1), y1, x1);
                        }
                    }
                    for (y1 = 0; y1 < CellsY; y1++)
                    {
                        newbub.SetValue(0, y1, 0);
                    }
                }
            }

            _bubbles = newbub;

            UpdateStatistics();
        }


        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(this);
            Transform tr = RenderTransform;
            pos = tr.Inverse.Transform(pos);

            pos += new Vector(-CellSize/2,-CellSize/2);
            pos = new Point(pos.X / CellSize, pos.Y / CellSize);
            int x = (int) Math.Round(pos.X);
            int y = (int) Math.Round(pos.Y);

            if (x < 0 || y < 0 || x >= CellsX || y >= CellsY)
            {
                SelectionCount = 0;
                SelectionPoints = 0;
                Selection = null;
                InvalidateVisual();
                return;
            }

            if (0 == (int) _bubbles.GetValue(y, x))
            {
                SelectionCount = 0;
                SelectionPoints = 0;
                Selection = null;
                InvalidateVisual();
                return;
            }

            if (null != Selection && (Selection.Contains(new Ball(x, y))))
            {
                _lastField = Array.CreateInstance(typeof(int), CellsY, CellsX);
                Array.Copy(_bubbles, _lastField, _bubbles.Length); // Copy..

                foreach (Ball b in Selection)
                {
                    _bubbles.SetValue(0, b.Y, b.X);
                }

                Score += SelectionPoints;

                MoveAll();

                SelectionCount = 0;
                SelectionPoints = 0;
                Selection = null;

                TurnCount = TurnCount + 1;

                if (CheckGameOver())
                {
                    Application.Current.Dispatcher.BeginInvoke(OnGameOverEvent, null);
                }
            }
            else
            {
                Selection = GetSelection(x, y);

                if (Selection.Count < 2)
                {
                    SelectionCount = 0;
                    SelectionPoints = 0;
                    Selection = null;
                    InvalidateVisual();
                    return;
                }

                SelectionCount = Selection.Count;
                SelectionPoints = Selection.Count * (Selection.Count - 1);
            }
            CheckUndo();
            InvalidateVisual();
        }

        private delegate void OnGameOverDelegate();

        private OnGameOverDelegate OnGameOverEvent;

        private void OnGameOver()
        {
            MessageBox.Show("Your result: " + Score.ToString(), "Game over");
            NewGame();
        }
    }

    public static class HSVColorConverter
    {
        public static void HsvToRgb(double h, double S, double V, out int r, out int g, out int b)
        {
            double H = h;
            while (H < 0) H += 360;
            while (H >= 360) H -= 360;
            double R, G, B;
            if (V <= 0) R = G = B = 0;
            else if (S <= 0) R = G = B = V;
            else
            {
                double hf = H / 60.0;
                int i = (int) Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {
                    case 0:
                        R = V; G = tv; B = pv;
                        break;
                    case 1:
                        R = qv; G = V; B = pv;
                        break;
                    case 2:
                        R = pv; G = V; B = tv;
                        break;
                    case 3:
                        R = pv; G = qv; B = V;
                        break;
                    case 4:
                        R = tv; G = pv; B = V;
                        break;
                    case 5:
                        R = V; G = pv; B = qv;
                        break;
                    case 6:
                        R = V; G = tv; B = pv;
                        break;
                    case -1:
                        R = V; G = pv; B = qv;
                        break;
                    default:
                        R = G = B = V;
                        break;
                }
            }
            r = Clamp((int) (R * 255.0));
            g = Clamp((int) (G * 255.0));
            b = Clamp((int) (B * 255.0));
        }

        public static int Clamp(int i)
        {
            return (i < 0) ? 0 : (i > 255 ? 255 : i);
        }
    }
}
