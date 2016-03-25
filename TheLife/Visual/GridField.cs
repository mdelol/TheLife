using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;
using static System.Double;

namespace TheLife.Visual
{
    public class GridField
    {
        private readonly Image _drawable;
        private readonly int _width;
        private readonly int _height;
        private readonly int _imageWidth;
        private readonly int _imageHeight;

        private List<IGameObject> _objects = new List<IGameObject>();
        private int _redraws;
        private Mutex _mut;

        public void Clear()
        {
            _mut.WaitOne();
            _objects.Clear();
            _mut.ReleaseMutex();
        }

        public GridField(Image drawable, int width, int height, int imageWidth, int imageHeight, Label fps)
        {
            _mut = new Mutex(true);
            _drawable = drawable;
            _width = width;
            _height = height;
            _imageWidth = imageWidth;
            _imageHeight = imageHeight;
            var i = 0;
            var thread = new Thread(() =>
            {
                bool t = true;
                var startTime = DateTime.UtcNow;
                while (t)
                {
                    var dateTime = DateTime.UtcNow;
                    TimeSpan tr = dateTime - startTime;
                    startTime = dateTime;
                    int secondsSinceEpoch = (int)tr.TotalMilliseconds;
                    t = Redraw(secondsSinceEpoch);
                    _redraws++;
                }

            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
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
            _mut.ReleaseMutex();
        }

        public bool Redraw(long timePassed)
        {
            try
            {
                var renderTargetBitmap = new RenderTargetBitmap(_imageWidth, _imageHeight, 100, 100, PixelFormats.Pbgra32);
                var xUnit = _imageWidth / _width;
                var yUnit = _imageHeight / _height;
                Grid grid = new Grid();
                DrawGrid(grid);
                grid.Arrange(new Rect(new Size(_imageWidth, _imageHeight)));
                _mut.WaitOne();
                _objects.ForEach(x =>
                {
                    x.Tick(timePassed);
                    var drawable = x.GetDrawable(xUnit, yUnit);
                    ((Grid)drawable.Parent)?.Children.Remove(drawable);
                    grid.Children.Add(drawable);
                });
                grid.UpdateLayout();


                renderTargetBitmap.Render(grid);
                _mut.ReleaseMutex();

                renderTargetBitmap.Freeze();

                _drawable.Dispatcher.Invoke(() =>
                {
                    _drawable.Source = renderTargetBitmap;
                });
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            return true;

        }

        public void RemoveGameObject(IGameObject gObject)
        {
            _mut.WaitOne();
            _objects.Remove(gObject);
            _mut.ReleaseMutex();
        }

        public void AddGameObject(IGameObject gObject)
        {
            _mut.WaitOne();
            _objects.Add(gObject);
            _mut.ReleaseMutex();
        }

        private void DrawGrid(Grid bitmap)
        {
            var deltaX = _imageWidth / _width;
            for (int i = 0; i != _width; i++)
            {
                var myLine = new Line();
                myLine.Stroke = Brushes.Black;
                var x = i * deltaX;
                myLine.X1 = x;
                myLine.X2 = x;
                myLine.Y1 = 0;
                myLine.Y2 = _imageHeight;
                myLine.Arrange(new Rect(new Size(_imageWidth, _imageHeight)));
                bitmap.Children.Add(myLine);
            }

            var deltaY = _imageHeight / _height;
            for (int i = 0; i != _width; i++)
            {
                var myLine = new Line();
                myLine.Stroke = Brushes.Black;
                var y = i * deltaY;
                myLine.Y1 = y;
                myLine.Y2 = y;
                myLine.X1 = 0;
                myLine.X2 = _imageWidth;
                myLine.Arrange(new Rect(new Size(_imageWidth, _imageHeight)));
                bitmap.Children.Add(myLine);
            }
        }

    }
}
