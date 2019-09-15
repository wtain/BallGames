using System.Collections.Generic;
using System.Windows.Media;

namespace bub.Visual.Detail
{
    public class Selection
    {
        private List<Ball> _selection;

        public List<Ball> Items { get { return _selection; } }

        public int Count { get { return Items.Count; } }

        public bool Contains(int x, int y)
        {
            return _selection.Contains(new Ball(x, y));
        }

        public void Add(int x, int y)
        {
            _selection.Add(new Ball(x, y));
        }

        public Selection()
        {
            _selection = new List<Ball>();
        }

        public bool IsSelected(int x, int y)
        {
            if (null == _selection)
                return false;
            foreach (Ball ball in _selection)
            {
                if (ball.X == x && ball.Y == y)
                    return true;
            }
            return false;
        }

        public void Render(DrawingContext drawingContext, Brush b, Pen p)
        {
            foreach (Ball ball in _selection)
                ball.RenderSelected(drawingContext, b, p);
        }
    }
}
