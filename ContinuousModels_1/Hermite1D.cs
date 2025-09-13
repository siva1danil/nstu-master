namespace SmoothingSpline2D;

// Кубические Эрмиты на [0,1].
// H00 — значение слева, H10 — производная слева,
// H01 — значение справа, H11 — производная справа.
public static class Hermite1D {
    public static double H00(double t) => 2 * t * t * t - 3 * t * t + 1;
    public static double H10(double t) => t * t * t - 2 * t * t + t;
    public static double H01(double t) => -2 * t * t * t + 3 * t * t;
    public static double H11(double t) => t * t * t - t * t;

    public static double dH00_dt(double t) => 6 * t * t - 6 * t;
    public static double dH10_dt(double t) => 3 * t * t - 4 * t + 1;
    public static double dH01_dt(double t) => -6 * t * t + 6 * t;
    public static double dH11_dt(double t) => 3 * t * t - 2 * t;

    public static double d2H00_dt2(double t) => 12 * t - 6;
    public static double d2H10_dt2(double t) => 6 * t - 4;
    public static double d2H01_dt2(double t) => -12 * t + 6;
    public static double d2H11_dt2(double t) => 6 * t - 2;
}
