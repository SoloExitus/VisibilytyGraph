using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Xml.Linq;

namespace VisibilityGraph
{
    public partial class Form1 : Form
    {

        List<List<PointFP>> Polygons;
        List<PointFP> GlobalVertexes;
        List<edge> GlobalEdges;

        List<int> TraversalOrder;

        float[,] VisabilityMatrix;

        Graphics graph;


        System.Drawing.SolidBrush FigureBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Yellow);
        System.Drawing.SolidBrush NodeBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
        System.Drawing.SolidBrush RedBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
        System.Drawing.SolidBrush GreenBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green);
        Pen LineBrush = new Pen(Brushes.Red);

        class edge
        {
            public PointF a = new PointF(0,0);
            public PointF b = new PointF(0,0);

            public edge(PointF A, PointF B)
            {
                a = A;
                b = B;

            }

            public bool isEdgeVertex(PointF t)
            {
                bool res = a == t || b == t;
                return res;
            }

        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Text files(*.xml)|*.xml|All files(*.*)|*.*";

            Polygons = new List<List<PointFP>>();
            GlobalVertexes = new List<PointFP>();
            GlobalEdges = new List<edge>();

            TraversalOrder = new List<int>();

            graph = CreateGraphics();
        }

        private int bypass(List<PointFP> pol)
        {
            if (pol.Count() < 3)
                return 1;
            float res = SideVectorPoint(pol[1].ToPointF(),pol[0].ToPointF(),pol[2].ToPointF());
            if (res > 0)
                return 1;
            return -1;
        }

        private void LoadFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;

            string filename = openFileDialog1.FileName;
            XDocument xdoc = XDocument.Load(filename);

            Polygons.Clear();
            GlobalVertexes.Clear();
            GlobalEdges.Clear();
            TraversalOrder.Clear();

            int polygonCounter = 0;
            int globalPointCounter = 0;
            foreach (XElement polygonElement in xdoc.Element("polygons").Elements("polygon"))
            {
                List<PointFP> polygonVertexes = new List<PointFP>();
                int pointCounter = 0;
                foreach (XElement pointElement in polygonElement.Elements("point"))
                {
                    XAttribute pointX = pointElement.Attribute("x");
                    XAttribute pointY = pointElement.Attribute("y");

                    if (pointX != null && pointY != null)
                    {
                        PointFP Vertex = new PointFP((float)pointX, (float)pointY, polygonCounter, pointCounter, globalPointCounter);
                        polygonVertexes.Add(Vertex);
                        GlobalVertexes.Add(Vertex);
                        pointCounter++;
                        globalPointCounter++;
                    }
                }

                TraversalOrder.Add(bypass(polygonVertexes));
                Polygons.Add(polygonVertexes);
                polygonCounter++;


            }

            foreach (List<PointFP> polygon in Polygons)
            {
                for (int i = 0; i < polygon.Count(); i++)
                {
                    GlobalEdges.Add(new edge(polygon[i].ToPointF(), polygon[(i+1)% polygon.Count()].ToPointF()));
                }
            }

            VisabilityMatrix = new float[GlobalVertexes.Count(), GlobalVertexes.Count()];

            DrawScene();
        }

        private PointF[] ToArray(List<PointFP> l)
        {
            List<PointF> t = new List<PointF>();

            foreach( PointFP p in l)
            {
                t.Add(p.ToPointF());
            }
            return t.ToArray();
        }

        private void DrawScene()
        {
            graph.Clear(Color.White);
            foreach (List<PointFP> polygon in Polygons)
            {
                if (polygon.Count > 2) // Make a polygons
                    graph.FillPolygon(FigureBrush, ToArray(polygon));
                foreach (PointFP node in polygon) //Draw all points
                    graph.FillRectangle(NodeBrush, node.X - 2, node.Y - 2, 4, 4);
            }
        }

        private void DrawWays()
        {
            int lenght = GlobalVertexes.Count();
            for (int i = 0; i < lenght; i++)
            {
                if(VisabilityMatrix[i, i] != float.MaxValue)
                    graph.FillRectangle(GreenBrush, GlobalVertexes[i].X - 2, GlobalVertexes[i].Y - 2, 4, 4);
                else
                    graph.FillRectangle(RedBrush, GlobalVertexes[i].X - 2, GlobalVertexes[i].Y - 2, 4, 4);
            
                for (int j = i+1; j < lenght; j++)
                {
                    if (VisabilityMatrix[i, j] != float.MaxValue)
                    {
                        graph.DrawLine(new Pen(GreenBrush), GlobalVertexes[i].ToPointF(), GlobalVertexes[j].ToPointF());
                    }
                    else
                    { }
                      //graph.DrawLine(new Pen(RedBrush), GlobalVertexes[i].ToPointF(), GlobalVertexes[j].ToPointF());
                }
            }
        }

        class PointFP
        {
            public float X = 0;
            public float Y = 0;
            public float angle = 0;
            public int polygonIndex = 0;
            public int pointIndex = 0;
            public int globalIndex = 0;

            public PointFP()
            {
            }

            public PointFP(PointF point, PointF center)
            {
                X = point.X;
                Y = point.Y;
                angle = (float)Math.Atan2(point.X - center.X, point.Y - center.Y);
            }

            public void RecalculateAngle(PointF center)
            {
                angle = (float)Math.Atan2(X - center.X, Y - center.Y);
            }

            public PointFP(PointFP p)
            {
                X = p.X;
                Y = p.Y;
                angle = p.angle;
            }

            public PointFP(PointF p)
            {
                X = p.X;
                Y = p.Y;
                angle = (float)Math.Atan2(p.Y, p.X);
            }

            public PointFP(float x, float y, int poly, int index,int glob)
            {
                X = x;
                Y = y;
                polygonIndex = poly;
                pointIndex = index;
                globalIndex = glob;
                angle = (float)Math.Atan2(Y, X);
            }

            public PointF ToPointF()
            {
                return new PointF(X, Y);
            }
        
        }

        private bool isConvexVertex(PointFP t, List<List<PointFP>> polygons)
        {
            List<PointFP> polygon = polygons[t.polygonIndex];
            return SideVectorPoint(polygon[(t.pointIndex + polygon.Count() - 1) % polygon.Count()].ToPointF(),
                                   polygon[(t.pointIndex + 1) % polygon.Count()].ToPointF(), t.ToPointF())* TraversalOrder[t.polygonIndex] >= 0;
        }

        //точно рабочий метод xD
        //private bool ConvexVertex(int index, List<PointFP> pol)
        //{
        //    return SideVectorPoint(pol[(pol.Count() + index - 1) % pol.Count()].ToPointF(), pol[(index + 1) % pol.Count()].ToPointF(), pol[index].ToPointF()) < 0;
        //}

        class AngleComparer : IComparer<PointFP>
        {
            public int Compare(PointFP f, PointFP s)
            {
                return f.angle.CompareTo(s.angle);
            }
        }

        class XComparer : IComparer<PointFP>
        {
            public int Compare(PointFP f, PointFP s)
            {
                return f.X.CompareTo(s.X);
            }
        }

        private float SideVectorPoint(PointF A, PointF B, PointF P)
        {
            PointF VecU = new PointF(B.X - A.X, B.Y - A.Y);
            PointF VecV = new PointF(P.X - A.X, P.Y - A.Y);

            return (VecU.X * VecV.Y - VecU.Y * VecV.X);
        }

        private bool between(double a, double b, double c)
        {
            return Math.Min(a, b) <= c && c >= Math.Max(a, b);
        }

        private float squareDistance(PointF f, PointF s)
        {
            return (s.X - f.X) * (s.X - f.X) + (s.Y - f.Y) * (s.Y - f.Y);
        }

        private bool isIntersection(PointF StartF, PointF EndF, edge testEdge)
        {
            PointF StartS = testEdge.a;
            PointF EndS = testEdge.b;

            if (EndF == StartS || EndF == EndS)
                return false;

            if (StartF == StartS || StartF == EndS)
                return false;

            double det = (EndF.X - StartF.X) * (StartS.Y - EndS.Y) -
                (EndF.Y - StartF.Y) * (StartS.X - EndS.X);

            if (det == 0.0)
            {
                
                //double firstK = (StartF.Y - EndF.Y) / (StartF.X - EndF.X);
                //double firstB = EndF.Y - firstK * EndF.X;
                //double secondK = (StartS.Y - EndS.Y) / (StartS.X - EndS.X);
                //double secondB = EndS.Y - secondK * EndS.X;

                //if (firstB == secondB)
                //{
                //    // проверяем проекцию точек на ОХ
                //    return between(StartF.X, EndF.X, StartS.X) ||
                //        between(StartF.X, EndF.X, EndS.X) ||
                //        between(StartS.X, EndS.X, StartF.X) ||
                //        between(StartS.X, EndS.X, EndF.X);
                //}

                return false;
            }

            double det1 = (StartS.X - StartF.X) * (StartS.Y - EndS.Y) -
                (StartS.Y - StartF.Y) * (StartS.X - EndS.X);

            double det2 = (EndF.X - StartF.X) * (StartS.Y - StartF.Y) -
                (EndF.Y - StartF.Y) * (StartS.X - StartF.X);

            double t = det1 / det;
            double r = det2 / det;

            if ((0 <= t) && (t <= 1) && (0 <= r) && (r <= 1))
            {
                return true;
            }
            return false;
        }

        private bool isSectorVertex(PointFP v, PointFP t)
        {

            List<PointFP> polygon = Polygons[v.polygonIndex];

            PointFP left = polygon[(polygon.Count() + v.pointIndex - 1) % polygon.Count()];
            PointFP right = polygon[(v.pointIndex + 1) % polygon.Count()];

            float lr = SideVectorPoint(v.ToPointF(), left.ToPointF(), t.ToPointF());
            float rr = SideVectorPoint(v.ToPointF(), right.ToPointF(), t.ToPointF());

            lr *= TraversalOrder[v.polygonIndex];
            rr *= TraversalOrder[v.polygonIndex];

            if (lr > 0 && rr < 0) return true;

            return false;
        }

        private bool LuchSegment(PointF v,PointF w, edge testEdge)
        {

            PointF StartF = v;
            PointF EndF = w;

            PointF StartS = testEdge.a;
            PointF EndS = testEdge.b;


            if (EndF == StartS || EndF == EndS)
                return false;

            if (StartF == StartS || StartF == EndS)
                return false;

            double det = (EndF.X - StartF.X) * (StartS.Y - EndS.Y) -
                (EndF.Y - StartF.Y) * (StartS.X - EndS.X);

            // Have not collision with segment
            if (det == 0.0)
                return false;

            double det1 = (StartS.X - StartF.X) * (StartS.Y - EndS.Y) -
                (StartS.Y - StartF.Y) * (StartS.X - EndS.X);

            double det2 = (EndF.X - StartF.X) * (StartS.Y - StartF.Y) -
                (EndF.Y - StartF.Y) * (StartS.X - StartF.X);

            double t = det1 / det;
            double r = det2 / det;

            if ((0 <= t)  && (0 <= r) && (r <= 1))
            {
                return true;
            }
            return false;
        }

        private List<edge> GetIncidentEdges(PointFP p)
        {
            List<edge> Edges = new List<edge>();

            List<PointFP> polygon = Polygons[p.polygonIndex];

            edge first = new edge(polygon[(polygon.Count() + p.pointIndex - 1)% polygon.Count()].ToPointF(), p.ToPointF());
            edge second = new edge(p.ToPointF(), polygon[(p.pointIndex + 1)% polygon.Count()].ToPointF());

            Edges.Add(first);
            Edges.Add(second);

            return Edges;
        }

        private void visibility_Click(object sender, EventArgs e)
        {
            if (GlobalVertexes.Count() < 3)
                return;

            List<PointFP> VOrder = new List<PointFP>(GlobalVertexes);

            VOrder.Sort(new XComparer());

            for(int i=0;i< VOrder.Count(); i++)
            {
                PointFP v = VOrder[i];

                if (isConvexVertex(v, Polygons))
                {
                    // работает алгоритм видимости для вершины
                    List<PointFP> OrderVertexes = new List<PointFP>(GlobalVertexes);

                    OrderVertexes.ForEach((point) =>
                        {
                            point.RecalculateAngle(v.ToPointF());
                        }
                    );

                    OrderVertexes.Sort(new AngleComparer());

                    List<PointFP> ClearOrderVertex = new List<PointFP>();

                    foreach(PointFP p in OrderVertexes)
                    {
                        if ( 0 < p.angle)
                            ClearOrderVertex.Add(p);
                    }
                    

                    List<edge> status = new List<edge>();

                    PointFP w = new PointFP(v);
                    w.Y += 1;

                    for (int s=0;s<GlobalEdges.Count();s++)
                    {
                        if (LuchSegment(v.ToPointF(), w.ToPointF(), GlobalEdges[s]))
                            status.Add(GlobalEdges[s]);
                    }

                    for(int k=0;k< ClearOrderVertex.Count();k++)
                    {
                        PointFP point = ClearOrderVertex[k];

                        if (point.globalIndex == v.globalIndex)
                            continue;

                        List<edge> Incident = GetIncidentEdges(point);

                        Incident.ForEach((edge) =>
                            {
                                if (!edge.isEdgeVertex(v.ToPointF()) )
                                {
                                    if (status.Contains(edge))
                                        status.Remove(edge);
                                    else
                                        status.Add(edge);
                                }
                            }
                        );

                        bool visibility = true;

                        // проверить что point не лежит в конусе образованном ребрами V
                        if (isSectorVertex(v, point) || isSectorVertex(point, v))
                        {
                            visibility = false;
                        }
                        
                        for(int j = 0; j < status.Count() && visibility; j++)
                        {

                            if (isIntersection(v.ToPointF(), point.ToPointF(), status[j]))
                            {
                                visibility = false;
                            }
                                
                        }

                        if (visibility)
                        {
                            float dist = (float)Math.Sqrt((double)(point.X - v.X) * (point.X - v.X) + (double)(point.Y - v.Y) * (point.Y - v.Y));
                            VisabilityMatrix[v.globalIndex, point.globalIndex] = dist;
                            VisabilityMatrix[point.globalIndex, v.globalIndex] = dist;
                            // записать в матрицу в клетку [i][k] расстояние между v и point
                            // и в [k][i]
                        }
                        else
                        {
                            VisabilityMatrix[v.globalIndex, point.globalIndex] = float.MaxValue;
                            VisabilityMatrix[point.globalIndex, v.globalIndex] = float.MaxValue;
                            // максимальное число в записать в ячейки
                        }

                        VisabilityMatrix[v.globalIndex, v.globalIndex] = 0;
                    }

                }
                
            }

            for (int t = 0; t < GlobalVertexes.Count(); t++)
            {
                if (!isConvexVertex(GlobalVertexes[t], Polygons))
                    for(int ts =0;ts< GlobalVertexes.Count();ts++)
                    {
                        //if (t == ts)
                        //    continue;
                       
                        VisabilityMatrix[ts, t] = float.MaxValue;
                        VisabilityMatrix[t, ts] = float.MaxValue;
                    }
                
            }

            DrawWays();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
