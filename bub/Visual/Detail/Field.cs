using bub.Helpers;
using bub.Visual.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Linq;

namespace bub.Visual.Detail
{
    public class Field
        : Matrix<int>
    {
        private static Color[] _colors;

        public int NumTotal
        {
            get { return Width * Height; }
        }

        public int NumNonEmpty
        {
            get { return data.Cast<int>().Count(n => 0 != n); }
        }

        static Field()
        {
            _colors = new Color[Constants.NumberOfColors + 1];
            _colors[0] = Colors.Black;

            for (int i = 1; i <= Constants.NumberOfColors; i++)
            {
                int r, g, b;
                HSVColorConverter.HsvToRgb(360.0 * i / Constants.NumberOfColors, 1, 1, out r, out g, out b);
                _colors[i] = Color.FromRgb((byte)r, (byte)g, (byte)b);
            }
        }

        public Field(int w, int h)
            : base(w, h)
        {
        }

        public Field(Field copy)
            : base(copy)
        {
        }

        public List<BallStats> CalcStats()
        {
            var stats = new List<BallStats>(Constants.NumberOfColors);
            for (int i = 0; i < Constants.NumberOfColors; i++)
                stats.Add(new BallStats(_colors[i + 1]));
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int color = this[x, y];
                    if (0 == color)
                        continue;
                    stats[color - 1].Increment();
                }
            }
            stats.Sort();
            return stats;
        }

        public void Generate()
        {
            var rnd = new Random();
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    this[x, y] = rnd.Next(1, Constants.NumberOfColors + 1);
        }

        public void ZeroField()
        {
            Set(0);
        }

        private void MoveAllImpl(Field sourceField)
        {
            for (int x = 0; x < Width; x++)
            {
                int y1 = Height - 1;
                for (int y = Height - 1; y >= 0; y--)
                {
                    int color = sourceField[x, y];
                    if (0 != color)
                    {
                        this[x, y1] = color;
                        y1--;
                    }
                }
                if (Height - 1 == y1)
                {
                    for (int x1 = x; x1 > 0; x1--)
                        for (y1 = 0; y1 < Height; y1++)
                            this[x1, y1] = this[x1 - 1, y1];
                    for (y1 = 0; y1 < Height; y1++)
                        this[0, y1] = 0;
                }
            }
        }

        public Field MoveAll()
        {
            var newField = new Field(Width, Height);

            newField.ZeroField();
            newField.MoveAllImpl(this);

            return newField;
        }

        public bool CheckGameOver()
        {
            for (int y = 0; y < Height - 1; y++)
            {
                for (int x = 0; x < Width - 1; x++)
                {
                    int color = this[x, y];
                    if (0 == color)
                        continue;
                    int color1 = this[x, y + 1];
                    int color2 = this[x + 1, y];
                    if (color1 == color || color2 == color)
                        return false;
                }
            }
            for (int y = 0; y < Height - 1; y++)
            {
                int color = this[Width - 1, y];
                if (0 == color)
                    continue;
                int color1 = this[Width - 1, y + 1];
                if (color1 == color)
                    return false;
            }
            for (int x = 0; x < Width - 1; x++)
            {
                int color = this[x, Height - 1];
                if (0 == color)
                    continue;
                int color1 = this[x + 1, Height - 1];
                if (color1 == color)
                    return false;
            }
            return true;
        }

        public void Render(DrawingContext drawingContext, Transform tr, double AnimationValue, IBubbler bubbler)
        {
            Brush brush = new SolidColorBrush(Colors.LightGray);
            drawingContext.DrawRectangle(brush, null, tr.TransformBounds(new Rect(0, 0, Constants.CellsX * Constants.CellSize, Constants.CellsY * Constants.CellSize)));
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int color = this[x, y];
                    if (0 == color)
                        continue;
                    Color borderColor = Color.Multiply(_colors[color], 0.2f);
                    borderColor.A = 224;
                    var br = new RadialGradientBrush(_colors[color], borderColor);
                    var ballPen = new Pen(br, 0.0);
                    bool isSelected = bubbler.IsSelected(x, y);

                    drawingContext.DrawEllipse(br, ballPen,
                                                tr.Transform(new Point(x * Constants.CellSize + Constants.CellSize / 2.0, y * Constants.CellSize + (isSelected ? AnimationValue * Constants.JumpDistance : 0.0) + Constants.CellSize / 2.0)),
                                                Constants.BubbleRadius + (isSelected ? AnimationValue * Constants.ShakeRadius : 0.0),
                                                Constants.BubbleRadius - (isSelected ? AnimationValue * Constants.ShakeRadius : 0.0));
                }
            }
        }

        private void SelectRecursive(Selection selection, Mask mask, int px, int py, int color)
        {
            if (mask.IsSet(px, py))
                return;
            if (this[px, py] != color)
                return;
            mask.Set(px, py);
            selection.Add(px, py);
            if (px - 1 >= 0)
                SelectRecursive(selection, mask, px - 1, py, color);
            if (px + 1 < Width)
                SelectRecursive(selection, mask, px + 1, py, color);
            if (py - 1 >= 0)
                SelectRecursive(selection, mask, px, py - 1, color);
            if (py + 1 < Height)
                SelectRecursive(selection, mask, px, py + 1, color);
        }

        public Selection GetSelection(int px, int py)
        {
            var selection = new Selection();
            var mask = new Mask(Width, Height);

            SelectRecursive(selection, mask, px, py, this[px, py]);

            return selection;
        }

        public bool IsBall(int x, int y)
        {
            return 0 != this[x, y];
        }

        public void EraseBalls(Selection selection)
        {
            foreach (Ball b in selection.Items)
                this[b.X, b.Y] = 0;
        }
    }
}
