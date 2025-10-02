using System.Globalization;

namespace ModernProblems_1;

public class Program {
    public static void TestContaminated(int seed, int n, double mu_clean, double sigma_clean, double mu_noise, double sigma_noise, double eps = 0.1) {
        ILocationScale clean = new ParetoNormal(v: 2.5, mu: mu_clean, sigma: sigma_clean);
        ILocationScale noise = new ParetoNormal(v: 2.5, mu: mu_noise, sigma: sigma_noise);
        ILocationScale dist = new Contaminated(clean, noise, eps: eps);
        Random r = new(seed);

        double[] x = new double[n];
        for (int i = 0; i < x.Length; i++)
            x[i] = dist.Sample(r);

        Console.WriteLine("Теоретическая дистрибуция:");
        Console.WriteLine($"  Mean: {dist.Mean().ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"  Variance: {dist.Variance().ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"  Skewness: {dist.Skewness().ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"  KurtosisExcess: {dist.KurtosisExcess().ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine("Практический мат стат:");
        Console.WriteLine($"  Mean: {MatStat.Mean(x).ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"  Variance: {MatStat.Variance(x).ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"  Skewness: {MatStat.Skewness(x).ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"  KurtosisExcess: {MatStat.KurtosisExcess(x).ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"  Median: {MatStat.Median(x).ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"  TrimmedMean(0.05): {MatStat.TrimmedMean(x, 0.05).ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"  TrimmedMean(0.1): {MatStat.TrimmedMean(x, 0.1).ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"  TrimmedMean(0.15): {MatStat.TrimmedMean(x, 0.15).ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"  MLE of the shift parameter: {MatStat.MaximumLikelihoodEstimatesOfTheShiftParameter(clean, x).ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"  GeneralizedRadicalAssessment(0.1): {MatStat.GeneralizedRadicalAssessment(clean, x, 0.1).ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"  GeneralizedRadicalAssessment(0.5): {MatStat.GeneralizedRadicalAssessment(clean, x, 0.5).ToString("F4", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"  GeneralizedRadicalAssessment(1.0): {MatStat.GeneralizedRadicalAssessment(clean, x, 1.0).ToString("F4", CultureInfo.InvariantCulture)}");
    }

    public static void Main(string[] args) {
        Console.WriteLine("Input parameters for contaminated distribution:");

        Console.Write("  Enter seed: ");
        int seed = int.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);

        Console.Write("  Enter n: ");
        int n = int.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);

        Console.Write("  Enter mu_clean: ");
        double mu_clean = double.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);

        Console.Write("  Enter sigma_clean: ");
        double sigma_clean = double.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);

        Console.Write("  Enter mu_noise: ");
        double mu_noise = double.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);

        Console.Write("  Enter sigma_noise: ");
        double sigma_noise = double.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);

        Console.Write("  Enter epsilon: ");
        double eps = double.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);

        TestContaminated(seed, n, mu_clean, sigma_clean, mu_noise, sigma_noise, eps);
    }
}