using System;

namespace bub.Visual.Detail
{
    public class Mask : Matrix<bool>
    {
        public Mask(int width, int height)
            : base(width, height)
        {
        }

        public bool IsSet(int x, int y)
        {
            return this[x, y];
        }

        public void Set(int x, int y)
        {
            this[x, y] = true;
        }
    }
}
