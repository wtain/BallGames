using System;

namespace bub.Visual.Detail
{
    public class Matrix<T>
        where T : new()
    {
        private Array data;
        private int width;
        private int height;

        public int Width { get { return width; } }
        public int Height { get { return height; } }

        public Matrix(int w, int h)
        {
            var defaultValue = new T();
            width = w;
            height = h;
            data = Array.CreateInstance(typeof(T), h, w);
            Set(defaultValue);
        }

        public Matrix(Matrix<T> copy)
            : this(copy.Width, copy.Height)
        {
            Array.Copy(copy.data, data, data.Length);
        }

        public void Set(T value)
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    this[x, y] = value;
        }

        public T this[int x, int y]
        {
            get { return (T)data.GetValue(y, x); }
            set { data.SetValue(value, y, x); }
        }
            
    }
}
