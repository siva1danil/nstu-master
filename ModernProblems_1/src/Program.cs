using System.Globalization;

namespace ModernProblems_1;

public class Program {
    public static void TestContaminated() {
        ILocationScale clean = new ParetoNormal(v: 3.0, mu: -6.9, sigma: 1.0);
        ILocationScale noise = new ParetoNormal(v: 3.0, mu: 6.9, sigma: 10.0);
        ILocationScale dist = new Contaminated(clean, noise, eps: 0.1);
        Random r = new();

        double[] x = new double[10000000];
        for (int i = 0; i < x.Length; i++)
            x[i] = dist.Sample(r);

        Console.WriteLine("Щас будет дистрибуция");
        Console.WriteLine($"Mean: {dist.Mean().ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"Variance: {dist.Variance().ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"Skewness: {dist.Skewness().ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"KurtosisExcess: {dist.KurtosisExcess().ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine();

        Console.WriteLine("Щас будет мат стат");
        Console.WriteLine($"Mean: {MatStat.Mean(x).ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"Variance: {MatStat.Variance(x).ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"Skewness: {MatStat.Skewness(x).ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"KurtosisExcess: {MatStat.KurtosisExcess(x).ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"Median: {MatStat.Median(x).ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"TrimmedMean(0.1): {MatStat.TrimmedMean(x, 0.1).ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"MLE of the shift parameter: {MatStat.MaximumLikelihoodEstimatesOfTheShiftParameter(dist, x).ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"GeneralizedRadicalAssessment: {MatStat.GeneralizedRadicalAssessment(dist, x, 1.0).ToString("F4", CultureInfo.InvariantCulture)}");
    }

    public static void Main(string[] args) {
        TestContaminated();
    }
}