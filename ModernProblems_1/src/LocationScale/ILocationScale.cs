namespace ModernProblems_1;

public interface ILocationScale {
    double Mu { get; } // Параметр сдвига
    double Sigma { get; } // Параметр масштаба

    double Pdf(double x); // Плотность
    double Sample(Random rng); // Генерация одного сэмпла
    Dictionary<int, double> Moments(); // Словарь моментов

    double Mean(); // Мат.ожидание
    double Variance(); // Дисперсия
    double Skewness(); // Асимметрия
    double KurtosisExcess(); // Эксцесс
}