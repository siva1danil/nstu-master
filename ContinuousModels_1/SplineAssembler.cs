using MathNet.Numerics.LinearAlgebra.Double;

namespace SmoothingSpline2D;

public static class SplineAssembler {
    public static (SparseMatrix A, DenseVector b) Build(
        Mesh mesh, double alpha, double beta,
        Func<double, double, double>? u = null,
        Func<double, double, double>? ux = null,
        Func<double, double, double>? uy = null,
        Func<double, double, double>? lapU = null
    ) {
        int N = mesh.Nodes.Count * 4;
        var A = SparseMatrix.Create(N, N, 0);
        var b = DenseVector.Create(N, 0);

        foreach (var e in mesh.Elements) {
            var fe = new FeSpline(); fe.Init(mesh, e);

            for (int i = 1; i <= 16; i++) {
                int ii = GetMatrixPos(e.NodeIdx, i);

                for (int j = 1; j <= 16; j++) {
                    int jj = GetMatrixPos(e.NodeIdx, j);

                    double aij = 0;

                    // ∫ φ_i φ_j
                    for (int g = 0; g < fe.G.Length; g++) {
                        var (xg, yg) = fe.G[g];
                        double w = fe.W[g];
                        aij += w * fe.Phi(i, xg, yg) * fe.Phi(j, xg, yg);
                    }

                    // + α ∫ ∇φ_i · ∇φ_j
                    aij += alpha * fe.IntegrateGrad(i, j);

                    // + β ∫ Δφ_i Δφ_j
                    aij += beta * fe.IntegrateLap(i, j);

                    A[ii, jj] += aij;
                }

                // b_i
                double bi = 0;
                for (int g = 0; g < fe.G.Length; g++) {
                    var (xg, yg) = fe.G[g];
                    double w = fe.W[g];

                    // значение u
                    double ug = u != null ? u(xg, yg) : mesh.UhAt(e, xg, yg);
                    bi += w * fe.Phi(i, xg, yg) * ug;

                    if (ux != null && uy != null && alpha != 0) {
                        fe.Dphi(i, xg, yg, out double dphix, out double dphiy);
                        bi += w * alpha * (dphix * ux(xg, yg) + dphiy * uy(xg, yg));
                    }

                    if (lapU != null && beta != 0) {
                        double lap_phi = fe.Lap(i, xg, yg);
                        bi += w * beta * lap_phi * lapU(xg, yg);
                    }
                }

                b[ii] += bi;
            }
        }

        return (A, b);
    }

    static int GetMatrixPos(int[] nodes, int bfNum) {
        int b = bfNum - 1;
        int localNode = b / 4;
        int dof = b % 4;
        return nodes[localNode] * 4 + dof;
    }
}
