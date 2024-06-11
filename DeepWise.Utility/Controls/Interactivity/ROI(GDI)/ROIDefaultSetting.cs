using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepWise.Controls.Interactivity
{
    public static class ROIColors
    {
        public static Color Highlight = Color.LightGreen;
        public static Color HighlightDark = Color.DarkGreen;
        public static Color Default = Color.FromArgb(0, 127, 255);
        public static Color MakeTransparent(this Color c) => Color.FromArgb(25,c);
    }
    public static class ROIPens
    {
        const float defaultWidth = 1.5f;
        static readonly float[] defaultDashPattern = { 5, 5 };
        public static Pen Hightlight => _hightlight;
        public static Pen HightlightDash => _hightlightDash;
        public static Pen Default => _defaultPen;
        public static Pen DefaultDash => _defaultPenDash;

        private static Pen _hightlight = new Pen(ROIColors.Highlight, defaultWidth);
        private static Pen _hightlightDash = new Pen(ROIColors.Highlight, defaultWidth) { DashPattern = defaultDashPattern };
        private static Pen _defaultPen = new Pen(ROIColors.Default, defaultWidth);
        private static Pen _defaultPenDash = new Pen(ROIColors.Default, defaultWidth) { DashPattern = defaultDashPattern };
    }
    public static class ROIBrushes
    {
        const int defaultSemitransparentLevel = 25;
        public static Brush Hightlight => _hightlight;
        public static Brush HightlightSt => _hightlightSt;
        public static Brush Default => _defaultPen;
        public static Brush DefaultSt => _defaultPenSt;

        private static Brush _hightlight = new SolidBrush(ROIColors.Highlight);
        private static Brush _hightlightSt = new SolidBrush(Color.FromArgb(defaultSemitransparentLevel, ROIColors.Highlight));
        private static Brush _defaultPen = new SolidBrush(ROIColors.Default);
        private static Brush _defaultPenSt = new SolidBrush(Color.FromArgb(defaultSemitransparentLevel,ROIColors.Default));
    }
}
