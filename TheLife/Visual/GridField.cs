using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace TheLife.Visual
{
    public class GridField
    {
        private readonly Canvas _drawable;
        private readonly int _width;
        private readonly int _height;

        private List<GameObject> _objects = new List<GameObject>();
        private long _redrawTime;
        private int _redraws = 0;

        public GridField(Canvas drawable, int width, int height, Label fps)
        {
            _drawable = drawable;
            _width = width;
            _height = height;
            DrawGrid();
            new Thread(() =>
            {
                long i = 0;
                while (Redraw(i++ / 10))
                {
                    _redraws++;
                    var l = (1000 / 60) - _redrawTime;
                    if (l > 10)
                    {
                        Thread.Sleep((int)l);
                    }
                }
            }).Start();
            new Thread(() =>
            {
                var interrupted = false;
                while (!interrupted)
                {
                    try
                    {

                        fps.Dispatcher.Invoke(() =>
                        {
                            fps.Content = _redraws;
                            _redraws = 0;
                        });
                        Thread.Sleep(1000);
                    }
                    catch (TaskCanceledException e)
                    {
                        interrupted = true;
                    }
                }
            }).Start();
        }

        public bool Redraw(long timePassed)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var xUnit = _drawable.ActualWidth / _width;
            var yUnit = _drawable.ActualHeight / _height;
            try
            {
                _objects.ForEach(x =>
                {
                    x.Tick(timePassed);
                    if (x.HasChanged)
                    {

                        _drawable.Dispatcher.InvokeAsync(delegate
                        {
                            _drawable.Children.Remove(x.GetPreviousDrawable);
                            var drawable = x.GetDrawable(yUnit, xUnit);
                            Canvas.SetLeft(drawable, x.X * xUnit);
                            Canvas.SetTop(drawable, x.Y * yUnit);

                            _drawable.Children.Add(drawable);
                        });
                    }

                });
            }
            catch (TaskCanceledException e)
            {
                return false;
            }
            finally
            {
                watch.Stop();
            }
            _redrawTime = watch.ElapsedMilliseconds;
            return true;
        }

        public void AddGameObject(GameObject gObject)
        {
            _objects.Add(gObject);
        }

        private void DrawGrid()
        {
            var deltaX = _drawable.ActualWidth / _width;
            for (int i = 1; i != _width; i++)
            {
                var myLine = new Line();
                myLine.Stroke = System.Windows.Media.Brushes.Black;
                var x = i * deltaX;
                myLine.X1 = x;
                myLine.X2 = x;
                myLine.Y1 = 0;
                myLine.Y2 = _drawable.ActualHeight;
                _drawable.Children.Add(myLine);
            }

            var deltaY = _drawable.ActualHeight / _height;
            for (int i = 1; i != _width; i++)
            {
                var myLine = new Line();
                myLine.Stroke = System.Windows.Media.Brushes.Black;
                var y = i * deltaY;
                myLine.Y1 = y;
                myLine.Y2 = y;
                myLine.X1 = 0;
                myLine.X2 = _drawable.ActualWidth;
                _drawable.Children.Add(myLine);
            }
        }

    }

    public interface GameObject
    {
        double X { get; }
        double Y { get; }
        void Tick(long timePassed);
        Shape GetDrawable(double xMult, double yMult);
        Shape GetPreviousDrawable { get; }
        bool HasChanged { get; }
    }

    public class CircleGameObject : GameObject
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
            _x += timePassed / 1000D;
            _y += timePassed / 1000D;
        }

        public CircleGameObject(int height, int width, int startPositionX, int startPositionY)
        {
            _x = startPositionX;
            _y = startPositionY;
            _height = height;
            _width = width;
        }

        public Shape GetDrawable(double xMult, double yMult)
        {
            if(_lastDrawable == null)
            _lastDrawable = new Ellipse();
            _lastDrawable.Height = _height * xMult;
            _lastDrawable.Width = _width * yMult;
            _lastDrawable.Fill = new SolidColorBrush(Colors.Black);
            return _lastDrawable;
        }

        public Shape GetPreviousDrawable => _lastDrawable;
        public bool HasChanged => true;
    }
}