using System.Globalization;

namespace ModernProblems_1;

public class Program {
    public static void TestContaminated(double mu_clean, double sigma_clean, double mu_noise, double sigma_noise) {
        ILocationScale clean = new ParetoNormal(v: 2.5, mu: mu_clean, sigma: sigma_clean);
        ILocationScale noise = new ParetoNormal(v: 2.5, mu: mu_noise, sigma: sigma_noise);
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
        Console.Write("Enter mu_clean: ");
        double mu_clean = double.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);

        Console.Write("Enter sigma_clean: ");
        double sigma_clean = double.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);

        Console.Write("Enter mu_noise: ");
        double mu_noise = double.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);

        Console.Write("Enter sigma_noise: ");
        double sigma_noise = double.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);

        TestContaminated(mu_clean, sigma_clean, mu_noise, sigma_noise);
    }
}