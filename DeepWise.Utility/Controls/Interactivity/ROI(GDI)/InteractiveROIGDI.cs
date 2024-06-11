using DeepWise.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepWise.Controls.Interactivity.ROI
{
    public abstract class InteractiveROIGDI : InteractiveObjectGDI
    {
        public abstract IShape GetShape();
        public abstract void SetShape(IShape shape);
        public abstract void SetShape(Point str, Point end);
    }
}
