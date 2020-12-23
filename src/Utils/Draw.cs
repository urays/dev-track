using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace IDevTrack.Utils
{
    public static class Draw
    {
        public static GraphicsPath CreateRoundRect(float x, float y, float width, float height, float radius)
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddLine(x + radius, y, x + width - (radius * 2), y);
            gp.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90);

            gp.AddLine(x + width, y + radius, x + width, y + height - (radius * 2));
            gp.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90);

            gp.AddLine(x + width - (radius * 2), y + height, x + radius, y + height);
            gp.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90);

            gp.AddLine(x, y + height - (radius * 2), x, y + radius);
            gp.AddArc(x, y, radius * 2, radius * 2, 180, 90);

            gp.CloseFigure();
            return gp;
        }

        public static GraphicsPath CreateRect(float x, float y, float width, float height)
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddLine(x, y, x + width, y);
            gp.AddLine(x + width, y, x + width, y + height);
            gp.AddLine(x + width, y + height, x, y + height);
            gp.AddLine(x, y + height, x, y);

            gp.CloseFigure();
            return gp;
        }

        /// <summary>
        /// 获取混合色
        /// </summary>
        /// <param name="clr1">基本颜色1</param>
        /// <param name="clr2">基本颜色2</param>
        /// <param name="alpha">混合比例</param>
        /// <returns>混合色</returns>
        public static Color MixingColors(Color clr1, Color clr2, int alpha)
        {
            int r = (clr2.R * alpha) / 255 + clr1.R * (255 - alpha) / 255;
            int g = (clr2.G * alpha) / 255 + clr1.G * (255 - alpha) / 255;
            int b = (clr2.B * alpha) / 255 + clr1.B * (255 - alpha) / 255;

            return Color.FromArgb(255, r, g, b);
        }

        public static Color Light(Color baseColor, byte value)
        {
            byte R = baseColor.R;
            byte G = baseColor.G;
            byte B = baseColor.B;

            if ((R + value) > 255) R = 255;
            else R += value;

            if ((G + value) > 255) G = 255;
            else G += value;

            if ((B + value) > 255) B = 255;
            else B += value;

            return Color.FromArgb(R, G, B);
        }

        public static Color Dark(Color baseColor, byte value)
        {
            byte R = baseColor.R;
            byte G = baseColor.G;
            byte B = baseColor.B;

            if ((R - value) < 0) R = 0;
            else R -= value;

            if ((G - value) < 0) G = 0;
            else G -= value;

            if ((B - value) < 0) B = 0;
            else B -= value;

            return Color.FromArgb(R, G, B);
        }

        public static Color ScaleAlpha(this Color c, float ratio)
        {
            return Color.FromArgb((byte)(c.A * ratio), c);
        }

        public static Color ColorFromHSV(float hue, float saturation, float value)
        {
            hue *= 360.0f;
            hue = (hue + 360.0f) % 360.0f;
            saturation = Math.Min(Math.Max(saturation, 0.0f), 1.0f);
            value = Math.Min(Math.Max(value, 0.0f), 1.0f);

            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }
    }
}