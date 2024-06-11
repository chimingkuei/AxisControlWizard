﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Media
{
    public static class ColorExtensions
    {
        public static float GetHue(this System.Windows.Media.Color c) =>
  System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B).GetHue();

        public static float GetBrightness(this System.Windows.Media.Color c) =>
          System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B).GetBrightness();

        public static float GetSaturation(this System.Windows.Media.Color c) =>
          System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B).GetSaturation();
    }
}
