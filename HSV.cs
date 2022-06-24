using System;
using TaleWorlds.Library;

namespace Int19h.Bannerlord.PettyKingdoms {

    internal struct HSV {
        public double H, S, V;

        public HSV(double h, double s, double v) {
            H = h;
            S = s;
            V = v;
        }

        public HSV(uint rgba)
            : this(Color.FromUint(rgba)) {

        }

        public HSV(Color color) {
            var r = color.Red;
            var g = color.Green;
            var b = color.Blue;
            V = Math.Max(Math.Max(r, g), b);
            var c = V - Math.Min(Math.Min(r, g), b);
            H =
                (c == 0) ? 0 :
                (V == r) ? (60) * (0 + (g - b) / c) :
                (V == g) ? (60) * (2 + (b - r) / c) :
                (V == b) ? (60) * (4 + (r - g) / c) :
                0;
            S = (V == 0) ? 0 : (c / V);
        }

        public Color ToRGB() {
            var c = V * S;
            var hi = (H / 60);
            var x = c * (1 - Math.Abs(hi % 2 - 1));
            double r, g, b;
            if (hi < 1) {
                (r, g, b) = (c, x, 0);
            } else if (hi < 2) {
                (r, g, b) = (x, c, 0);
            } else if (hi < 3) {
                (r, g, b) = (0, c, x);
            } else if (hi < 4) {
                (r, g, b) = (0, x, c);
            } else if (hi < 5) {
                (r, g, b) = (x, 0, c);
            } else {
                (r, g, b) = (c, 0, x);
            }
            var m = V - c;
            return new Color((float)(r + m), (float)(g + m), (float)(b + m));
        }

        public uint ToUInt() => ToRGB().ToUnsignedInteger();
    }
}
