using MathNet.Numerics;

namespace ModernProblems_1;

public sealed class MatStat {
    public static double Mean(double[] x) => x.Average();

    public static double Variance(double[] x) {
        double mean = Mean(x);
        return x.Select(xi => (xi - mean) * (xi - mean)).Average();
    }

    public static double Skewness(double[] x) {
        double m = Mean(x), s = Math.Sqrt(Variance(x));
        return x.Select(xi => Math.Pow((xi - m) / s, 3)).Average();
    }

    public static double KurtosisExcess(double[] x) {
        double m = Mean(x), s = Math.Sqrt(Variance(x));
        return x.Select(xi => Math.Pow((xi - m) / s, 4)).Average() - 3.0;
    }

    public static double Median(double[] x) {
        var sorted = x.OrderBy(xi => xi).ToArray();
        int n = sorted.Length;
        return n % 2 == 1 ? sorted[n / 2] : (sorted[(n / 2) - 1] + sorted[n / 2]) / 2.0;
    }

    public static double TrimmedMean(double[] x, double ratio) {
        var sorted = x.OrderBy(xi => xi).ToArray();
        int n = sorted.Length;
        int count = (int)(n * ratio / 2.0);
        return sorted.Skip(count).Take(n - 2 * count).Average();
    }

    public static double MaximumLikelihoodEstimatesOfTheShiftParameter(ILocationScale scale, double[] x) {
        int n = x.Length;
        var objective = new Func<double, double>(mu => -x.Select(xi => Math.Log(scale.Pdf(xi - mu + scale.Mu))).Sum());
        double result = FindMinimum.OfScalarFunction(objective, Median(x), 1e-5, 10000);
        return result;
    }

    public static double GeneralizedRadicalAssessment(ILocationScale scale, double[] x, double aboba) {
        var objective = new Func<double, double>(mu => -x.Select(xi => Math.Pow(scale.Pdf(xi - mu + scale.Mu), aboba)).Sum());
        double result = FindMinimum.OfScalarFunction(objective, Median(x), 1e-5, 10000);
        return result;
    }
}