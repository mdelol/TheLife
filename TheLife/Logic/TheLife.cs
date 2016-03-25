using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Shapes;
using TheLife.Visual;

namespace TheLife.Logic
{
    public class TheLife
    {
        private readonly int _width;
        private readonly int _height;
        private ICell[][] _cells;
        private GridField _graphics;
        readonly Random _random = new Random();


        private TheLife(int height, int width, GridField graphics)
        {
            _height = height;
            _width = width;
            _graphics = graphics;
            _cells = InitCells();
        }

        private ICell[][] InitCells()
        {
            var cells = new ICell[_width][];
            for (var i = 0; i != _width; i++)
            {
                cells[i] = new ICell[_height];
            }

            return cells;
        }
        private static Thread _starterThread;

        public static Thread Start(int height, int width, GridField graphics)
        {
            _starterThread = Thread.CurrentThread;
            var thread = new Thread(() =>
            {
                var theLife = new TheLife(height, width, graphics);
                theLife.GenerateRandom();
                theLife.StartInternalLoop();
            });

            thread.SetApartmentState(ApartmentState.STA);

            return thread;
        }

        private static int percent = 90;
        private void GenerateRandom()
        {
            for (int i = 0; i < _height /100D* percent; i++)
            {
                for (int j = 0; j < _width / 100D * percent; j++)
                {
                    var x = _random.Next(0, _width - 1);
                    var y = _random.Next(0, _height - 1);
                    _cells[x][y] = new Cell(1, 1, x, y);
                }
            }
        }

        private void StartInternalLoop()
        {
            while (_starterThread.IsAlive)
            {
                _graphics.Clear();
                for (int i = 0; i != _height; i++)
                {
                    for (int j = 0; j != _width; j++)
                    {
                        if (_cells[j][i] != null)
                        {
                            _graphics.AddGameObject(_cells[j][i]);

                        }
                    }
                }
                Iteration();
            }
        }

        private void Iteration()
        {
            var newCells = InitCells();
            for (int i = 0; i != _height; i++)
            {
                for (int j = 0; j != _width; j++)
                {
                    var xIndices = new List<int> { j - 1, j, j + 1 }.Select(x =>
                    {
                        if (x < 0) return _width - 1;
                        if (x == _width) return 0;
                        return x;
                    });
                    var yIndices = new List<int> { i - 1, i, i + 1 }.Select(x =>
                    {
                        if (x < 0) return _height - 1;
                        if (x == _height) return 0;
                        return x;
                    });
                    var selectMany = xIndices.SelectMany(x =>
                    {
                        return yIndices.Select(y => new Tuple<int, int>(x, y)).Where(e=>e.Item1 !=j || e.Item2 !=i);
                    });

                    if (_cells[j][i] == null)
                    {
                        if (selectMany.Count(x => _cells[x.Item1][x.Item2] != null) == 3)
                        {
                            newCells[j][i] = new Cell(1, 1, j, i);
                        }
                    }
                    else
                    {
                        var count = selectMany.Count(x => _cells[x.Item1][x.Item2] != null);
                        if (count == 3 || count ==2)
                        {
                            newCells[j][i] = _cells[j][i];
                        }
                    }
                }
            }
            _cells = newCells;
        }
    }

    public interface ICell : IGameObject
    {
        void StartFading();
        void StartExistence();
    }

    public class Cell : CircleGameObject, ICell
    {
        public double X { get; }
        public double Y { get; }

        public void StartFading()
        {
            throw new NotImplementedException();
        }

        public void StartExistence()
        {
            throw new NotImplementedException();
        }

        public Cell(int height, int width, int startPositionX, int startPositionY) : base(height, width, startPositionX, startPositionY)
        {
        }
    }
}