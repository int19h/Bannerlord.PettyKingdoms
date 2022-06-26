using System;
using TaleWorlds.Library;

namespace Int19h.Bannerlord.PettyKingdoms {

    internal struct HsvColor {
        public double Hue, Saturation, Value;

        public HsvColor(double hue, double saturation, double value) {
            Hue = hue;
            Saturation = saturation;
            Value = value;
        }

        public HsvColor(uint rgba)
            : this(Color.FromUint(rgba)) {

        }

        public HsvColor(Color color) {
            var r = color.Red;
            var g = color.Green;
            var b = color.Blue;
            Value = Math.Max(Math.Max(r, g), b);
            var c = Value - Math.Min(Math.Min(r, g), b);
            Hue =
                (c == 0) ? 0 :
                (Value == r) ? (60) * (0 + (g - b) / c) :
                (Value == g) ? (60) * (2 + (b - r) / c) :
                (Value == b) ? (60) * (4 + (r - g) / c) :
                0;
            Saturation = (Value == 0) ? 0 : (c / Value);
        }

        public Color ToColor() {
            var c = Value * Saturation;
            var hi = (Hue / 60);
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
            var m = Value - c;
            return new Color((float)(r + m), (float)(g + m), (float)(b + m));
        }

        public uint ToUInt32() => ToColor().ToUnsignedInteger();
    }
}
