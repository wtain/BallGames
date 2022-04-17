using bub.Data;
using bub.Dialogs;
using bub.Visual.Detail;
using bub.Visual.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace bub.Visual
{
    public class Bubbler : FrameworkElement, IBubbler
    {
        private Field _field;
        private Selection _selection;
        private GameState _prevState;

        private User currentUser;
        private GameDatabase database;

        public static readonly DependencyProperty CurrentStateProperty =
            DependencyProperty.Register("CurrentState", typeof(GameState), typeof(Bubbler));

        public static readonly DependencyProperty AnimationValueProperty =
            DependencyProperty.Register("AnimationValue", typeof(double), typeof(Bubbler),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ScoreProperty =
            DependencyProperty.Register("Score", typeof(int), typeof(Bubbler));

        public static readonly DependencyProperty MoveCountProperty =
            DependencyProperty.Register("MoveCount", typeof(int), typeof(Bubbler));

        public static readonly DependencyProperty BallStatisticsProperty =
            DependencyProperty.Register("BallStatistics", typeof(List<BallStats>), typeof(Bubbler));

        public static readonly DependencyProperty SelectionPointsProperty =
            DependencyProperty.Register("SelectionPoints", typeof(int), typeof(Bubbler));

        public static readonly DependencyProperty SelectionCountProperty =
            DependencyProperty.Register("SelectionCount", typeof(int), typeof(Bubbler));

        public static readonly DependencyProperty MaxScoreProperty =
            DependencyProperty.Register("MaxScore", typeof(int), typeof(Bubbler));

        public static readonly DependencyProperty CanUndoProperty =
            DependencyProperty.Register("CanUndo", typeof(bool), typeof(Bubbler));

        public GameState CurrentState
        {
            get { return (GameState)GetValue(CurrentStateProperty); }
            set { SetValue(CurrentStateProperty, value); }
        }

        public double AnimationValue
        {
            get { return (double)GetValue(AnimationValueProperty); }
            set { SetValue(AnimationValueProperty, value); }
        }

        private GameState PreviousState
        {
            get
            {
                if (null == _prevState)
                    _prevState = new GameState();
                return _prevState;
            }
        }

        public Selection Selection
        {
            get { return _selection; }
            set
            {
                if (null != _selection)
                    PreviousState.Selection = _selection;
                _selection = value;
            }
        }

        public int Score
        {
            get { return (int)GetValue(ScoreProperty); }
            set
            {
                PreviousState.Score = Score;
                SetValue(ScoreProperty, value);
            }
        }

        public int MaxScore
        {
            get { return (int)GetValue(MaxScoreProperty); }
            set { SetValue(MaxScoreProperty, value); }
        }

        public int MoveCount
        {
            get { return (int)GetValue(MoveCountProperty); }
            set
            {
                PreviousState.MoveCount = MoveCount;
                SetValue(MoveCountProperty, value);
            }
        }

        public List<BallStats> BallStatistics
        {
            get { return (List<BallStats>)GetValue(BallStatisticsProperty); }
            set { SetValue(BallStatisticsProperty, value); }
        }

        public int SelectionPoints
        {
            get { return (int)GetValue(SelectionPointsProperty); }
            set
            {
                PreviousState.SelectionPoints = SelectionPoints;
                SetValue(SelectionPointsProperty, value);
            }
        }

        public int SelectionCount
        {
            get { return (int)GetValue(SelectionCountProperty); }
            set
            {
                PreviousState.SelectionCount = SelectionCount;
                SetValue(SelectionCountProperty, value);
            }
        }

        private void UpdateStatistics()
        {
            BallStatistics = _field.CalcStats();
            MaxScore = new ScoreCalculator().CalculateMaxScore(_field);
        }

        public Bubbler()
        {
            try // Enable designer
            {
                database = new GameDatabase();
            }
            catch (Exception)
            {

            }

            _field = new Field(Constants.CellsX, Constants.CellsY);

            OnGameOverEvent = OnGameOver;

            Generate();
            Score = 0;

            var DA = new DoubleAnimation(Constants.ShakeFrom, Constants.ShakeTo, TimeSpan.FromSeconds(Constants.ShakePeriod));
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
            _prevState = null;
            MoveCount = 0;
            CheckUndo();
            InvalidateVisual();
        }

        private void Generate()
        {
            _field.Generate();
            UpdateStatistics();
        }

        public bool IsSelected(int x, int y)
        {
            return null != Selection && Selection.IsSelected(x, y);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            _field.Render(drawingContext, RenderTransform, AnimationValue, this);
            var c = Color.FromArgb(128, 32, 64, 128);
            var b = new SolidColorBrush(c);
            var p = new Pen(b, 0.0);
            Selection?.Render(drawingContext, b, p);
        }

        public void Undo()
        {
            _field = new Field(PreviousState.Field);
            Score = PreviousState.Score;
            Selection = PreviousState.Selection;
            SelectionCount = PreviousState.SelectionCount;
            SelectionPoints = PreviousState.SelectionPoints;
            MoveCount = PreviousState.MoveCount;
            UpdateStatistics();
            _prevState = null;
            CheckUndo();
            InvalidateVisual();
        }

        public bool CanUndo
        {
            get { return (bool)GetValue(CanUndoProperty); }
            protected set { SetValue(CanUndoProperty, value); }
        }

        private void CheckUndo()
        {
            CanUndo = (null != _prevState) && _prevState.Field != null;
        }

        private void MoveAll()
        {
            _field = _field.MoveAll();

            UpdateStatistics();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(this);
            Transform tr = RenderTransform;
            pos = tr.Inverse.Transform(pos);

            pos += new Vector(-Constants.CellSize / 2, -Constants.CellSize / 2);
            pos = new Point(pos.X / Constants.CellSize, pos.Y / Constants.CellSize);
            int x = (int)Math.Round(pos.X);
            int y = (int)Math.Round(pos.Y);

            if (x < 0 || y < 0 || x >= Constants.CellsX || y >= Constants.CellsY)
            {
                SelectionCount = 0;
                SelectionPoints = 0;
                Selection = null;
                InvalidateVisual();
                return;
            }

            if (!_field.IsBall(x, y))
            {
                SelectionCount = 0;
                SelectionPoints = 0;
                Selection = null;
                InvalidateVisual();
                return;
            }

            if (null != Selection && (Selection.Contains(x, y)))
            {
                PreviousState.Field = new Field(_field);

                _field.EraseBalls(Selection);

                Score += SelectionPoints;

                MoveAll();

                SelectionCount = 0;
                SelectionPoints = 0;
                Selection = null;

                MoveCount = MoveCount + 1;

                if (_field.CheckGameOver())
                    Application.Current.Dispatcher.BeginInvoke(OnGameOverEvent, null);
            }
            else
            {
                Selection = _field.GetSelection(x, y);

                if (Selection.Count < 2)
                {
                    SelectionCount = 0;
                    SelectionPoints = 0;
                    Selection = null;
                    InvalidateVisual();
                    return;
                }

                SelectionCount = Selection.Count;
                SelectionPoints = Selection.Points;
            }
            CheckUndo();
            InvalidateVisual();
        }

        private delegate void OnGameOverDelegate();

        private OnGameOverDelegate OnGameOverEvent;

        private void OnGameOver()
        {
            MessageBox.Show($"Your result: {Score}", "Game over");

            var users = new List<User>(database.Users);

            if (null == currentUser)
            {
                var dlg = new SelectUserDialog(new List<User>(database.Users));
                var dr = dlg.ShowDialog();
                if (dr.HasValue && dr.Value)
                {
                    currentUser = dlg.User;
                    if (dlg.IsNewUser)
                        currentUser.Id = database.AddUser(currentUser.Name);
                }
            }
            database.AddResult(currentUser, Score, DateTime.Now);

            NewGame();
        }

        public void ShowHighscores()
        {
            var win = new HighScoresWindow(database.GameResults);
            win.ShowDialog();
        }
    }
}
