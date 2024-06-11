using DeepWise.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepWise.Controls.Interactivity.Behaviors
{
    public class DefectViewer : DisplayBehavior
    {
        public DefectViewer(IEnumerable<IDefect> defects = null, Dictionary<string, System.Windows.Media.Brush> colors = null)
        {
            Defects = defects;
            if (colors != null)
            {
                foreach (var item in colors)
                    DefectColors[item.Key] = item.Value;
            }
        }

        public Dictionary<string, System.Windows.Media.Brush> DefectColors { get; } = new Dictionary<string, System.Windows.Media.Brush>()
        {
            {"_default", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Magenta)},
        };


        public void SetDefectColor(string defect, System.Windows.Media.Color color)
        {
            DefectColors[defect] = new System.Windows.Media.SolidColorBrush(color);
        }

        public override void Enter(Display display)
        {
            base.Enter(display);
            InitailizeMarks();
        }

        public override void Exist()
        {
            Display.InteractiveObjects.Clear();
            base.Exist();
        }

        void InitailizeMarks()
        {
            if (Display == null) return;
            Display.InteractiveObjects.Clear();
            if (Defects == null) return;
            foreach (var defect in Defects)
            {
                string name = defect.Name;
                if (name.Contains('['))
                    name = name.Substring(0, name.IndexOf('['));
                var mark = new RectMark(defect.Region, $"{defect.Name} : {defect.Region.Width}x{defect.Region.Height}");
                if (DefectColors.ContainsKey(name))
                    mark.Stroke = DefectColors[name];
                else
                    mark.Stroke = DefectColors["_default"];
                Display.InteractiveObjects.Add(mark);
            }
        }
        IEnumerable<IDefect> _defects;
        public IEnumerable<IDefect> Defects
        {
            get => _defects;
            set
            {
                if (_defects == value) return;
                _defects = value;
                InitailizeMarks();
            }
        }
        IEnumerable<IDefect> hide = null;

        public Predicate<IDefect> Filer { get; }

        public override void MouseDown(DisplayMouseEventArgs e)
        {
            if (Defects != null && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                hide = Defects;
                Defects = null;
            }
        }

        public override void MouseMove(DisplayMouseEventArgs e)
        {
            //throw new NotImplementedException();
        }

        public override void MouseUp(DisplayMouseEventArgs e)
        {
            //throw new NotImplementedException();
            if (hide != null && (e.LeftButton == System.Windows.Input.MouseButtonState.Released || e.RightButton == System.Windows.Input.MouseButtonState.Released))
            {
                Defects = hide;
                hide = null;
            }
        }
    }
}
