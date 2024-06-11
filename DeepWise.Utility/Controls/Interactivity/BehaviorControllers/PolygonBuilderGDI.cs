using DeepWise.Shapes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Point = DeepWise.Shapes.Point;
namespace DeepWise.Controls.Interactivity.BehaviorControllers
{
    public class PolygonBuilderGDI : DisplayControllerGDI
    {
        public Polygon Polygon => new Polygon(vertexs);
        public Color PreviewBorderColor { get; set; } = Color.LimeGreen;
        public override void Enter(DisplayGDI display)
        {
            base.Enter(display);
            ContextMenuStrip.Items.Add("刪除", null, DeleteSelectedNode);
        }
        private void DeleteSelectedNode(object sender, EventArgs e)
        {
            if (selectedVertexIndex != -1)
            {
                vertexs.RemoveAt(selectedVertexIndex);
                selectedVertexIndex = -1;
                BTN_Checked.Enabled = vertexs.Count > 2;
            }
            Display.PushNewFrame();
        }
        public override bool ShowButtons => true;
        private ContextMenuStrip ContextMenuStrip { get; } = new ContextMenuStrip();
        List<Point> vertexs = new List<Point>();
        int selectedVertexIndex = -1;
        public override void MouseDown(object sender, DisplayGDIMouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    for (int i = 0; i < vertexs.Count; i++)
                    {
                        if (e.IsMouseOver(vertexs[i]))
                        {
                            selectedVertexIndex = i;
                            return;
                        }
                    }
                    selectedVertexIndex = -1;

                    vertexs.Add(e.Location);
                    BTN_Checked.Enabled = vertexs.Count > 2;
                    e.Cancel = true;
                    Display.PushNewFrame();
                    break;
                case MouseButtons.Right:
                    bool hit = false;
                    for (int i = 0; i < vertexs.Count; i++)
                    {
                        if (e.IsMouseOver(vertexs[i]))
                        {
                            selectedVertexIndex = i;
                            hit = true;
                            break;
                        }
                    }

                    if (!hit)
                    {
                        selectedVertexIndex = -1;
                        return;
                    }

                    ContextMenuStrip.Show(Cursor.Position);
                    e.Cancel = true;
                    Display.PushNewFrame();
                    break;

            }


        }
        public override void MouseMove(object sender, DisplayGDIMouseEventArgs e)
        {
            if (selectedVertexIndex != -1)
            {
                vertexs[selectedVertexIndex] = e.Location;
                Display.PushNewFrame();
                e.Cancel = true;
                return;
            }

            foreach (var p in vertexs)
            {
                if (e.IsMouseOver(p))
                {
                    Display.Cursor = Cursors.Hand;
                    e.Cancel = true;
                    return;
                }
            }
        }
        public override void MouseUp(object sender, DisplayGDIMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && selectedVertexIndex != -1)
            {
                selectedVertexIndex = -1;
                Display.PushNewFrame();
            }
        }
        public override void DrawOverlay(DisplayGDIPaintEventArgs e)
        {
            if (vertexs.Count > 1)
            {
                e.Color = PreviewBorderColor;
                for (int i = 0; i < vertexs.Count - 1; i++)
                    e.DrawLine(vertexs[i], vertexs[i + 1]);
                if (vertexs.Count > 2)
                {
                    e.Color = Color.DarkGreen;
                    e.DrawLine(vertexs[vertexs.Count - 1], vertexs[0]);
                }


            }
            e.Color = Color.Red;
            foreach (Point p in vertexs) e.DrawCross(p);
            if (selectedVertexIndex != -1)
            {
                e.Color = Color.Yellow;
                e.DrawCross(vertexs[selectedVertexIndex]);
            }
            //throw new NotImplementedException();
        }
    }
}
