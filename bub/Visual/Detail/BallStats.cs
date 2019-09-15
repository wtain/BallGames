
using System;
using System.Windows.Media;

namespace bub.Visual.Detail
{
    public class BallStats : IComparable
    {
        private int m_Count;
        private Color m_Color;

        public int CompareTo(object obj)
        {
            BallStats b = obj as BallStats;
            if (null == b)
                return 0;
            return b.m_Count - m_Count;
        }

        public BallStats(Color Color)
        {
            m_Count = 0;
            m_Color = Color;
        }

        public int Count
        {
            get { return m_Count; }
            set { m_Count = value; }
        }

        public SolidColorBrush Color
        {
            get { return new SolidColorBrush(m_Color); }
        }

        public void Increment()
        {
            m_Count++;
        }
    }
}
