
using System.Windows;
using System.Windows.Media;

namespace bub.Visual.Detail
{
    public struct Ball
    {
        public int X;
        public int Y;

        public Ball(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void RenderSelected(DrawingContext drawingContext, Brush b, Pen p)
        {
            drawingContext.DrawRectangle(b, p, new Rect(X * Constants.CellSize, Y * Constants.CellSize, Constants.CellSize, Constants.CellSize));
        }
    }
}
