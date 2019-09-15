using System;

namespace bub.Helpers
{
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
                int i = (int)Math.Floor(hf);
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
            r = Clamp((int)(R * 255.0));
            g = Clamp((int)(G * 255.0));
            b = Clamp((int)(B * 255.0));
        }

        public static int Clamp(int i)
        {
            return (i < 0) ? 0 : (i > 255 ? 255 : i);
        }
    }
}
