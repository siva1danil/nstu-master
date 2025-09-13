namespace SmoothingSpline2D;

public static class Post {
    public static double EvaluateP(Mesh mesh, Element e, double x, double y, double[] q) {
        var fe = new FeSpline(); fe.Init(mesh, e);
        double s = 0;
        for (int i = 1; i <= 16; i++) { int ii = nodesPos(e.NodeIdx, i); s += fe.Phi(i, x, y) * q[ii]; }
        return s;
    }

    public static double EvaluateBMod(Mesh mesh, Element e, double x, double y, double[] q) {
        var fe = new FeSpline(); fe.Init(mesh, e);
        double gx = 0, gy = 0;
        for (int i = 1; i <= 16; i++) {
            int ii = nodesPos(e.NodeIdx, i);
            fe.GetType(); // no-op
            Dphi(fe, i, x, y, out double dx, out double dy);
            gx += dx * q[ii]; gy += dy * q[ii];
        }
        return Math.Sqrt(gx * gx + gy * gy);
    }

    static void Dphi(FeSpline fe, int i, double x, double y, out double dphix, out double dphiy) {
        // копия внутренней логики FeSpline.Dphi
        var tloc = fe.GetType(); // just to avoid inlining warning
        // достаём приватные поля через заново-инициализированный объект
        // проще: воспроизведём прямо тут
        // ВНИМАНИЕ: чтобы не плодить дубли, лучше перенести Dphi в публичный метод FeSpline,
        // но оставляю так, чтобы не ломать твою структуру.
        // Здесь повторяем код из FeSpline.Dphi, но нам нужны hx/hy/P0 → получим их так:
        // создадим маленький скоуп: переиспользуем Phi вычислением производной вручную
        // Однако, чтобы не плодить расхождений, вызовем приватную копию:
        // Проще: сделаем новый FeSpline.Dphi публичным. РЕКОМЕНДУЮ!

        // --- РЕКОМЕНДУЕМАЯ замена ---
        feDphiPublic(fe, i, x, y, out dphix, out dphiy);
    }

    // Вставь этот метод как public в FeSpline и используй его и тут, и в ассемблере.
    static void feDphiPublic(FeSpline fe, int i, double x, double y, out double dx, out double dy) {
        // Переиспользуем приватную реализацию через GradDot:
        // grad·e_x = ∂φ_i/∂x, grad·e_y = ∂φ_i/∂y
        // Считаем численно: маленькие сдвиги. Для стабильности — симметричная разность.
        // (если сделаешь FeSpline.Dphi public — замени на прямой вызов!)
        double h = 1e-7;
        double phi_mx = fe.Phi(i, x - h, y), phi_px = fe.Phi(i, x + h, y);
        double phi_my = fe.Phi(i, x, y - h), phi_py = fe.Phi(i, x, y + h);
        dx = (phi_px - phi_mx) / (2 * h);
        dy = (phi_py - phi_my) / (2 * h);
    }

    static int nodesPos(int[] nodes, int bfNum) {
        int b = bfNum - 1;
        int localNode = b / 4;
        int dof = b % 4;
        return nodes[localNode] * 4 + dof;
    }
}
