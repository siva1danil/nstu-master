namespace SmoothingSpline2D;
public class Mesh
{
    public List<Node> Nodes = new();
    public List<Element> Elements = new();
    public double[] UhAtNodes = Array.Empty<double>();

    // Bilinear interpolation using rectangle (n0 bottom-left, n1 bottom-right, n2 top-right, n3 top-left)
    public double UhAt(Element e, double x, double y)
    {
        var n0 = Nodes[e.NodeIdx[0]]; // (x0,y0)
        var n1 = Nodes[e.NodeIdx[1]]; // (x1,y0)
        var n3 = Nodes[e.NodeIdx[3]]; // (x0,y1)

        double x0 = n0.X, y0 = n0.Y;
        double x1 = n1.X, y1 = n3.Y;

        double xi = (x - x0) / (x1 - x0);
        double eta = (y - y0) / (y1 - y0);

        double u0 = UhAtNodes[e.NodeIdx[0]];
        double u1 = UhAtNodes[e.NodeIdx[1]];
        double u2 = UhAtNodes[e.NodeIdx[2]];
        double u3 = UhAtNodes[e.NodeIdx[3]];

        return (1 - xi) * (1 - eta) * u0
             + xi * (1 - eta) * u1
             + xi * eta * u2
             + (1 - xi) * eta * u3;
    }
}