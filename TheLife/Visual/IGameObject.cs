using System.Windows.Shapes;

namespace TheLife.Visual
{
    public interface IGameObject
    {
        double X { get; }
        double Y { get; }
        void Tick(long timePassed);
        Shape GetDrawable(double xMult, double yMult);
        Shape GetPreviousDrawable { get; }
        bool HasChanged { get; }
    }
}