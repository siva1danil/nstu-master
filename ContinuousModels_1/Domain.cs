namespace SmoothingSpline2D;

public record Node(double X, double Y);

public class Element {
    public int[] NodeIdx = new int[4]; // индексы узлов
}
