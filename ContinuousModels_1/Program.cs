using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Double.Solvers;
using MathNet.Numerics.LinearAlgebra.Solvers;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.ImageSharp;
using OxyPlot.Series;

using SmoothingSpline2D;

using Element = SmoothingSpline2D.Element;

class Program {
    static void ExportPlot(double[,] data, string filename) {
        var model = new PlotModel { Title = filename };
        var heatmap = new HeatMapSeries {
            X0 = 0, X1 = 1,
            Y0 = 0, Y1 = 1,
            Interpolate = true,
            RenderMethod = HeatMapRenderMethod.Bitmap,
            Data = data
        };
        model.Series.Add(heatmap);
        model.Axes.Add(new LinearColorAxis { Position = AxisPosition.Right });
        PngExporter.Export(model, filename, 600, 500);
    }

    static void Main() {
        int nx = 40, ny = 40;
        double alpha = 1e-2;
        double beta = 1e-4;
        var mesh = new Mesh();

        // 1. Узлы
        for (int j = 0; j <= ny; j++)
            for (int i = 0; i <= nx; i++)
                mesh.Nodes.Add(new Node(i / (double)nx, j / (double)ny));

        // 2. Элементы
        for (int j = 0; j < ny; j++)
            for (int i = 0; i < nx; i++) {
                int n0 = j * (nx + 1) + i;
                int n1 = n0 + 1;
                int n3 = n0 + (nx + 1);
                int n2 = n3 + 1;
                mesh.Elements.Add(new Element { NodeIdx = new[] { n0, n1, n2, n3 } });
            }

        // 3. Значения u_h в узлах (синусоида + линейная часть)
        mesh.UhAtNodes = mesh.Nodes
            .Select(n => n.X + n.Y)
            // .Select(n => Math.Sin(Math.PI * n.X) * Math.Sin(Math.PI * n.Y) + 0.1 * n.X)
            .ToArray();

        // 4. Сборка системы
        // var (A, b) = SplineAssembler.Build(mesh, alpha, beta);
        var (A, b) = SplineAssembler.Build(
    mesh, alpha, beta,
    u: (x, y) => Math.Sin(Math.PI * x) * Math.Sin(Math.PI * y) + 0.1 * x,
    ux: (x, y) => Math.PI * Math.Cos(Math.PI * x) * Math.Sin(Math.PI * y) + 0.1,
    uy: (x, y) => Math.PI * Math.Sin(Math.PI * x) * Math.Cos(Math.PI * y),
    lapU: (x, y) => -2.0 * Math.PI * Math.PI *
                   Math.Sin(Math.PI * x) * Math.Sin(Math.PI * y)  // (0.1x даёт 0)
);

        // 5. Решаем СЛАУ
        var x = new DenseVector(A.RowCount);
        var solver = new BiCgStab();
        var preconditioner = new DiagonalPreconditioner();
        preconditioner.Initialize(A);
        var criteria = new IIterationStopCriterion<double>[] {
            new ResidualStopCriterion<double>(1e-12),
            new IterationCountStopCriterion<double>(20000)
        };
        var iterator = new Iterator<double>(criteria);
        solver.Solve(A, b, x, iterator, preconditioner);
        var q = x.ToArray();

        // 6. Готовим сетку для картинок
        int gridN = 50;
        double[,] Utrue = new double[gridN + 1, gridN + 1];
        double[,] Uspline = new double[gridN + 1, gridN + 1];
        double[,] Diff = new double[gridN + 1, gridN + 1];
        double[,] BtrueMap = new double[gridN + 1, gridN + 1];
        double[,] BsplMap = new double[gridN + 1, gridN + 1];

        for (int j = 0; j <= gridN; j++)
            for (int i = 0; i <= gridN; i++) {
                double px = i / (double)gridN;
                double py = j / (double)gridN;

                var e = mesh.Elements.First(el => {
                    var n0 = mesh.Nodes[el.NodeIdx[0]];
                    var n2 = mesh.Nodes[el.NodeIdx[2]];
                    return px >= n0.X && px <= n2.X && py >= n0.Y && py <= n2.Y;
                });

                double utrue = Math.Sin(Math.PI * px) * Math.Sin(Math.PI * py) + 0.1 * px;
                double uspline = Post.EvaluateP(mesh, e, px, py, q);

                double ux = Math.PI * Math.Cos(Math.PI * px) * Math.Sin(Math.PI * py) + 0.1;
                double uy = Math.PI * Math.Sin(Math.PI * px) * Math.Cos(Math.PI * py);
                double btrue = Math.Sqrt(ux * ux + uy * uy);
                double bspl = Post.EvaluateBMod(mesh, e, px, py, q);

                Utrue[i, j] = utrue;
                Uspline[i, j] = uspline;
                Diff[i, j] = uspline - utrue;
                BtrueMap[i, j] = btrue;
                BsplMap[i, j] = bspl;
            }

        // 7. Экспорт PNG
        ExportPlot(Utrue, "Utrue.png");
        ExportPlot(Uspline, "Uspline.png");
        ExportPlot(Diff, "Diff.png");
        ExportPlot(BtrueMap, "Btrue.png");
        ExportPlot(BsplMap, "Bspline.png");

        Console.WriteLine("Графики сохранены: Utrue.png, Uspline.png, Diff.png, Btrue.png, Bspline.png");
    }
}
