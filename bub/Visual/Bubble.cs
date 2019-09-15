using System.Windows;
using System.Windows.Media;

namespace bub.Visual
{
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
    }
}
