using CellarAutomatonLib;
using Microsoft.Win32;
using OxyPlot;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PmeVisualizationWpf
{
    public class MainViewModel : DataViewModel
    {
        public MainViewModel()
        {
            CanvasItemsSource = new ObservableCollection<Shape>();
            GraphsItemSource = new ObservableCollection<GraphViewModel>();
            CanvasHeight = 1000;
            CanvasWidth = 1000;
            _brushes = new[]{
                Brushes.Gray,
                Brushes.Blue,
                Brushes.Red,
                Brushes.Green,
                Brushes.Black,
                Brushes.Olive,
            };
            _colors = _brushes
                .Select(_ => _.Color)
                .ToArray();

        }

        private readonly SolidColorBrush[] _brushes;

        private readonly Color[] _colors;


        private Config _config;
        private int[,,] _caList;
        protected override void OnPathSelect(object obj = null)
        {
            FileDialog fd = new OpenFileDialog();
            fd.Filter = "*.cfg|*.*";
            if (fd.ShowDialog() == true)
            {
                ProjectPath = fd.FileName;
                var text = File.ReadAllText(ProjectPath);
                _config = Config.Deserialize(text);
                var caLogPath = _config.Paths[Config.CaLogName];
                _caList = new int[_config.Height, _config.Width, _config.StepCount + 3];
                var i = 0;
                var j = 0;
                var n = 0;
                foreach (var line in File.ReadLines(caLogPath))
                {
                    foreach (var item in line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        _caList[j, i++, n] = int.Parse(item);

                    i = 0;
                    j++;

                    if (j == _config.Height)
                    {
                        j = 0;
                        n++;
                    }
                }

                var cellWidth = CanvasWidth / _config.Width;
                var cellHeight = CanvasHeight / _config.Height;
                for (var h = 0; h < _config.Height; h++)
                    for (var k = 0; k < _config.Width; k++)
                        CanvasItemsSource.Add(new Rectangle
                        {
                            Width = cellWidth,
                            Height = cellHeight,
                            Margin = new Thickness(cellWidth * k, cellHeight * h, 0, 0),
                            Fill = Brushes.White
                        });

                var stateGraphsPath = _config.Paths[Config.StateGraphsName];
                var gd = GraphsDescriber.Deserialize(File.ReadAllText(stateGraphsPath));
                foreach (var state in gd.StateGraphs.Keys)
                {
                    GraphsItemSource.Add(new GraphViewModel
                    {
                        Name = _config.States[state].Name,
                        Color = _colors[state],
                        Values = gd.StateGraphs[state].Select((d, c) => new DataPoint(c, d)).ToList()
                    });
                }

                Step = 0;
                Draw(Step);
            }
        }

        protected override void OnNext(object obj = null)
        {
            if (Step == _config.StepCount - 1)
                return;
            Step++;
            Draw(Step);
        }

        protected override void OnPrevious(object obj = null)
        {
            if (Step == 0)
                return;
            Step--;
            Draw(Step);
        }

        protected override void OnPlay(object obj = null)
        {
            Task.Run(() =>
            {
                while (Step < _config.StepCount)
                {
                    Draw(Step++);
                    Task.Delay(50).Wait();
                }
            });
        }

        private void Draw(int n)
        {
            var t = Application.Current.Dispatcher.InvokeAsync(delegate
            {
                for (var h = 0; h < _config.Height; h++)
                    for (var k = 0; k < _config.Width; k++)
                        if (CanvasItemsSource[_config.Width * h + k].Fill != _brushes[_caList[h, k, n]])
                            CanvasItemsSource[_config.Width * h + k].Fill = _brushes[_caList[h, k, n]];
            });
            t.Wait();
        }

    }
}