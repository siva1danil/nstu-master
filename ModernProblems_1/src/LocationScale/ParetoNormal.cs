namespace ModernProblems_1;

public sealed class ParetoNormal(double v, double mu = 0.0, double sigma = 1.0) : ILocationScale {
    public double Mu { get; } = mu;
    public double Sigma { get; } = sigma;
    public double V { get; } = v;

    public double Pdf(double x) {
        double z = (x - Mu) / Sigma;
        double nz = Math.Abs(z);
        double k_nu = K(V);

        double num = nz <= V ? SmallEf(z) : SmallEf(V) * Math.Pow(V / nz, V * V);
        return (num / k_nu) / Sigma;
    }

    public double Sample(Random rng) {
        double k_nu = K(V);
        double p_center = (2.0 * BigEf(V) - 1.0) / k_nu;
        double u = rng.NextDouble();
        double z;

        if (u <= p_center) {
            do {
                z = StdNormal(rng);
            } while (Math.Abs(z) > V);
        } else {
            double r = Math.Max(double.Epsilon, rng.NextDouble());
            double alpha = V * V - 1.0;
            double xtail = V * Math.Pow(1.0 - r, -1.0 / alpha);
            z = (rng.NextDouble() < 0.5) ? xtail : -xtail;
        }

        return Mu + Sigma * z;
    }

    public Dictionary<int, double> Moments() {
        var dict = new Dictionary<int, double>();

        double m0 = 1.0;
        double m2 = MomentZEven(2);
        double m4 = MomentZEven(4);

        dict[0] = m0;
        dict[1] = Mu;
        dict[2] = Mu * Mu + Sigma * Sigma * m2;
        dict[3] = Mu * Mu * Mu + 3.0 * Mu * Sigma * Sigma * m2;
        dict[4] = Math.Pow(Mu, 4) + 6.0 * Mu * Mu * Sigma * Sigma * m2 + Math.Pow(Sigma, 4) * m4;

        return dict;
    }


    public double Mean() {
        return Mu;
    }

    public double Variance() {
        var moments = Moments();
        return moments[2] - moments[1] * moments[1];
    }

    public double Skewness() {
        var m = Moments();
        double mean = m[1];
        double sigma = Math.Sqrt(m[2] - mean * mean);

        double mu3 = m[3] - 3 * m[2] * mean + 2 * Math.Pow(mean, 3);
        return mu3 / Math.Pow(sigma, 3);
    }

    public double KurtosisExcess() {
        var m = Moments();
        double mean = m[1];
        double sigma = Math.Sqrt(m[2] - mean * mean);

        double mu4 = m[4] - 4 * m[3] * mean + 6 * m[2] * mean * mean - 3 * Math.Pow(mean, 4);
        return mu4 / Math.Pow(sigma, 4) - 3;
    }


    private static double K(double nu) => 2.0 * BigEf(nu) - 1.0 + (2.0 * nu * SmallEf(nu)) / (nu * nu - 1.0);
    private static double BigEf(double x) => 0.5 * (1.0 + Erf(x / Math.Sqrt(2.0)));
    private static double SmallEf(double x) => Math.Exp(-0.5 * x * x) / Math.Sqrt(2.0 * Math.PI);
    private static double StdNormal(Random rng) => Math.Sqrt(-2.0 * Math.Log(Math.Max(double.Epsilon, rng.NextDouble()))) * Math.Cos(2.0 * Math.PI * rng.NextDouble());
    private static double Erf(double x) {
        double sign = Math.Sign(x);
        x = Math.Abs(x);
        double t = 1.0 / (1.0 + 0.3275911 * x);
        double[] a = [0.254829592, -0.284496736, 1.421413741, -1.453152027, 1.061405429];
        double poly = (((a[4] * t + a[3]) * t + a[2]) * t + a[1]) * t + a[0];
        double y = 1.0 - poly * Math.Exp(-x * x);
        return sign * y;
    }
    private double MomentZEven(int k) {
        double k_nu = K(V);
        double small_ef_nu = SmallEf(V);
        double big_ef_nu = BigEf(V);

        double center = k == 2
            ? (2.0 * big_ef_nu - 1.0) - 2.0 * V * small_ef_nu
            : 6.0 * big_ef_nu - 3.0 - 2.0 * (V * V * V + 3.0 * V) * small_ef_nu;

        double denom = V * V - (k + 1);
        if (denom <= 0.0) return double.NaN;

        double tails = 2.0 * small_ef_nu * Math.Pow(V, k + 1.0) / (k_nu * denom);
        return (center / k_nu) + tails;
    }
}