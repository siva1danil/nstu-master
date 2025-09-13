namespace SmoothingSpline2D;

public class FeSpline {
    public Mesh Mesh = null!;
    public Element E = null!;
    double hx, hy;
    Node P0 = null!;

    // Публичные точки/веса Гаусса
    private readonly (double x, double y)[] _G = new (double, double)[12];
    private readonly double[] _W = new double[12];
    public (double x, double y)[] G => _G;
    public double[] W => _W;

    public void Init(Mesh mesh, Element e) {
        Mesh = mesh; E = e;
        var n0 = mesh.Nodes[e.NodeIdx[0]]; // BL
        var n1 = mesh.Nodes[e.NodeIdx[1]]; // BR
        var n3 = mesh.Nodes[e.NodeIdx[3]]; // TL
        P0 = n0;
        hx = n1.X - n0.X;
        hy = n3.Y - n0.Y;
        InitGauss12();
    }

    (double tx, double ty) ToLocal(double x, double y) => ((x - P0.X) / hx, (y - P0.Y) / hy);
    public (double x, double y) ToGlobal(double tx, double ty) => (P0.X + tx * hx, P0.Y + ty * hy);

    // ----- ЯВНОЕ сопоставление 16 базисов -----
    // Локальные узлы: 0:BL(0,0), 1:BR(1,0), 2:TR(1,1), 3:TL(0,1)
    // Внутри узла DOF: 0: value, 1: d/dx, 2: d/dy, 3: d2/dxdy
    static void Decode(int i, out int ln, out int ldof) {
        int b = i - 1;
        ln = b / 4;        // 0..3
        ldof = b % 4;        // 0..3
    }

    static void EndFuncsX(int ln, out int idxVal, out int idxDer) // какие H.. по X у этого узла
    {
        // узлы 0 и 3 = x-слева (t_x=0) → (H00,H10)
        // узлы 1 и 2 = x-справа (t_x=1) → (H01,H11)
        if (ln == 0 || ln == 3) { idxVal = 0; idxDer = 1; } // (H00,H10)
        else { idxVal = 2; idxDer = 3; } // (H01,H11)
    }
    static void EndFuncsY(int ln, out int idyVal, out int idyDer) // какие H.. по Y у этого узла
    {
        // узлы 0 и 1 = y-снизу (t_y=0) → (H00,H10)
        // узлы 2 и 3 = y-сверху (t_y=1) → (H01,H11)
        if (ln == 0 || ln == 1) { idyVal = 0; idyDer = 1; } // (H00,H10)
        else { idyVal = 2; idyDer = 3; } // (H01,H11)
    }

    // Возврат пары функций по индексу: 0:H00, 1:H10, 2:H01, 3:H11
    static double H(int idx, double t) => idx switch { 0 => Hermite1D.H00(t), 1 => Hermite1D.H10(t), 2 => Hermite1D.H01(t), 3 => Hermite1D.H11(t), _ => 0 };
    static double dHdt(int idx, double t) => idx switch { 0 => Hermite1D.dH00_dt(t), 1 => Hermite1D.dH10_dt(t), 2 => Hermite1D.dH01_dt(t), 3 => Hermite1D.dH11_dt(t), _ => 0 };
    static double d2Hdt2(int idx, double t) => idx switch { 0 => Hermite1D.d2H00_dt2(t), 1 => Hermite1D.d2H10_dt2(t), 2 => Hermite1D.d2H01_dt2(t), 3 => Hermite1D.d2H11_dt2(t), _ => 0 };

    // φ_i
    public double Phi(int i, double x, double y) {
        var (tx, ty) = ToLocal(x, y);
        Decode(i, out int ln, out int dof);
        EndFuncsX(ln, out int ixVal, out int ixDer);
        EndFuncsY(ln, out int iyVal, out int iyDer);

        double X = dof switch {
            0 or 2 => H(ixVal, tx), // value or d/dy: по X берём значение
            1 or 3 => H(ixDer, tx), // d/dx or d2:   по X берём производную
        };
        double Y = dof switch {
            0 or 1 => H(iyVal, ty), // value or d/dx: по Y берём значение
            2 or 3 => H(iyDer, ty), // d/dy or d2:     по Y берём производную
        };

        return X * Y;
    }

    // ∂φ_i/∂x и ∂φ_i/∂y
    public void Dphi(int i, double x, double y, out double dphix, out double dphiy) {
        var (tx, ty) = ToLocal(x, y);
        Decode(i, out int ln, out int dof);
        EndFuncsX(ln, out int ixVal, out int ixDer);
        EndFuncsY(ln, out int iyVal, out int iyDer);

        double dXdx = (dof == 1 || dof == 3 ? dHdt(ixDer, tx) : dHdt(ixVal, tx)) / hx; // если по X «деривативный» базис — дифференцируем его
        double X = (dof == 1 || dof == 3 ? H(ixDer, tx) : H(ixVal, tx));

        double dYdy = (dof == 2 || dof == 3 ? dHdt(iyDer, ty) : dHdt(iyVal, ty)) / hy;
        double Y = (dof == 2 || dof == 3 ? H(iyDer, ty) : H(iyVal, ty));

        dphix = dXdx * Y;
        dphiy = X * dYdy;
    }

    public double GradDot(int i, int j, double x, double y) {
        Dphi(i, x, y, out double ix, out double iy);
        Dphi(j, x, y, out double jx, out double jy);
        return ix * jx + iy * jy;
    }

    public double Lap(int i, double x, double y) {
        var (tx, ty) = ToLocal(x, y);
        Decode(i, out int ln, out int dof);
        EndFuncsX(ln, out int ixVal, out int ixDer);
        EndFuncsY(ln, out int iyVal, out int iyDer);

        // d²/dx²
        double d2Xdx2 = (dof == 1 || dof == 3 ? d2Hdt2(ixDer, tx) : d2Hdt2(ixVal, tx)) / (hx * hx);
        double Y0 = (dof == 2 || dof == 3 ? H(iyDer, ty) : H(iyVal, ty));
        // d²/dy²
        double X0 = (dof == 1 || dof == 3 ? H(ixDer, tx) : H(ixVal, tx));
        double d2Ydy2 = (dof == 2 || dof == 3 ? d2Hdt2(iyDer, ty) : d2Hdt2(iyVal, ty)) / (hy * hy);

        return d2Xdx2 * Y0 + X0 * d2Ydy2;
    }

    // ------- Гаусс 12-точек (с Якобианом hx*hy*1/4) -------
    void InitGauss12() {
        double a = Math.Sqrt((114.0 - 3.0 * Math.Sqrt(583.0)) / 287.0);
        double b = Math.Sqrt((114.0 + 3.0 * Math.Sqrt(583.0)) / 287.0);
        double c = Math.Sqrt(6.0 / 7.0);
        double wa = 307.0 / 810.0 + 923.0 / (270.0 * Math.Sqrt(583.0));
        double wb = 307.0 / 810.0 - 923.0 / (270.0 * Math.Sqrt(583.0));
        double wc = 98.0 / 405.0;

        var loc = new (double x, double y)[] {
            (-c,0),( c,0),(0,-c),(0, c),
            (-a,-a),( a,-a),(-a, a),( a, a),
            (-b,-b),( b,-b),(-b, b),( b, b)
        };
        double scale = 0.25 * hx * hy;

        for (int i = 0; i < 12; i++) {
            var (sx, sy) = loc[i];
            var (X, Y) = ToGlobal(0.5 * (sx + 1), 0.5 * (sy + 1));
            _G[i] = (X, Y);
            double basew = (i < 4 ? wc : (i < 8 ? wa : wb));
            _W[i] = basew * scale;
        }
    }

    public double IntegrateGrad(int i, int j) { double s = 0; for (int g = 0; g < 12; g++) { var (x, y) = _G[g]; s += _W[g] * GradDot(i, j, x, y); } return s; }
    public double IntegrateLap(int i, int j) { double s = 0; for (int g = 0; g < 12; g++) { var (x, y) = _G[g]; s += _W[g] * Lap(i, x, y) * Lap(j, x, y); } return s; }
}
