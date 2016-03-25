using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TheLife.Visual
{
    public class CircleGameObject : IGameObject
    {
        private readonly int _height;
        private readonly int _width;
        private Ellipse _lastDrawable;
        private double _x;
        private double _y;

        public double X
        {
            get { return _x; }
        }

        public double Y
        {
            get { return _y; }
        }

        public void Tick(long timePassed)
        {
            //_x += timePassed / 1000D;
            //_y += timePassed / 1000D;
        }

        public CircleGameObject(int height, int width, int startPositionX, int startPositionY)
        {
            _x = startPositionX;
            _y = startPositionY;
            _height = height;
            _width = width;
        }

        protected double Hm = 1;
        public Shape GetDrawable(double xMult, double yMult)
        {
            if (_lastDrawable == null)
            {
                _lastDrawable = new Ellipse();
                _lastDrawable.VerticalAlignment = VerticalAlignment.Top;
                _lastDrawable.HorizontalAlignment = HorizontalAlignment.Left;
                _lastDrawable.Fill = new SolidColorBrush(Colors.Red);
            }
            _lastDrawable.Height = _height * xMult * Hm;
            _lastDrawable.Width = _width * yMult * Hm;
            _lastDrawable.Margin = new Thickness(_x * xMult, _y * yMult, 0, 0);
            return _lastDrawable;
        }

        public Shape GetPreviousDrawable => _lastDrawable;
        public bool HasChanged => true;
    }
}