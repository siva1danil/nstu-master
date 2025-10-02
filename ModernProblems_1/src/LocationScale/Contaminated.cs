namespace ModernProblems_1;

class Contaminated(ILocationScale clean, ILocationScale noise, double eps) : ILocationScale {
    public ILocationScale Clean { get; } = clean;
    public ILocationScale Noise { get; } = noise;
    public double Eps { get; } = eps;

    public double Mu => (1 - Eps) * Clean.Mu + Eps * Noise.Mu;
    public double Sigma => double.NaN;

    public double Pdf(double x) => (1 - Eps) * Clean.Pdf(x) + Eps * Noise.Pdf(x);
    public double Sample(Random rng) => new Random(Hash(rng, "mask")).NextDouble() < Eps ? Noise.Sample(new Random(Hash(rng, "noise"))) : Clean.Sample(new Random(Hash(rng, "clean")));
    public Dictionary<int, double> Moments() => throw new NotImplementedException();

    public double Mean() => (1 - Eps) * Clean.Mean() + Eps * Noise.Mean();
    public double Variance() => Eps * (Math.Pow(Noise.Mean(), 2) + Noise.Variance()) + (1 - Eps) * (Math.Pow(Clean.Mean(), 2) + Clean.Variance()) - Math.Pow(Mean(), 2);
    public double Skewness() {
        double m = Mean();
        double v = Variance();
        double s = (1 - Eps) * (Math.Pow(Clean.Mean() - m, 3) + 3 * Clean.Variance() * (Clean.Mean() - m) + Math.Pow(Clean.Variance(), 1.5) * Clean.Skewness())
            + Eps * (Math.Pow(Noise.Mean() - m, 3) + 3 * Noise.Variance() * (Noise.Mean() - m) + Math.Pow(Noise.Variance(), 1.5) * Noise.Skewness());
        return s / Math.Pow(v, 1.5);
    }
    public double KurtosisExcess() {
        double m = Mean();
        double v = Variance();
        double k = (1 - Eps) * (Math.Pow(Clean.Mean() - m, 4) + 6 * Clean.Variance() * Math.Pow(Clean.Mean() - m, 2) + 4 * Math.Pow(Clean.Variance(), 1.5) * Clean.Skewness() * (Clean.Mean() - m) + Math.Pow(Clean.Variance(), 2) * (Clean.KurtosisExcess() + 3))
            + Eps * (Math.Pow(Noise.Mean() - m, 4) + 6 * Noise.Variance() * Math.Pow(Noise.Mean() - m, 2) + 4 * Math.Pow(Noise.Variance(), 1.5) * Noise.Skewness() * (Noise.Mean() - m) + Math.Pow(Noise.Variance(), 2) * (Noise.KurtosisExcess() + 3));
        return k / (v * v) - 3.0;
    }

    private static int Hash(Random rng, string salt) {
        int baseVal = rng.Next();
        unchecked {
            int h = baseVal;
            foreach (var c in salt)
                h = h * 31 + c;
            return h & 0x7fffffff;
        }
    }
}