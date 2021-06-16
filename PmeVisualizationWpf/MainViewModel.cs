using CellarAutomatonLib;
using Microsoft.Win32;
using OxyPlot;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PmeVisualizationWpf
{
    public class MainViewModel : DataViewModel
    {
        public MainViewModel()
        {
            IsNotProcessing = true;
            GraphsItemSource = new ObservableCollection<GraphViewModel>();
            _colors = new[]
            {
                Color.FromArgb(255, 0, 0, 0),
                Color.FromArgb(255, 100, 100, 100),
                Color.FromArgb(255, 255, 0, 0),
                Color.FromArgb(255, 0, 255, 0),
                Color.FromArgb(255, 0, 0, 255),
                Color.FromArgb(255, 255, 255, 0),
                Color.FromArgb(255, 255, 0, 255),
                Color.FromArgb(255, 0, 255, 255),
                Color.FromArgb(255, 255, 255, 255),
            };
        }

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
                Draw(Step, null);
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
            Draw(Step, false);
        }

        private CancellationTokenSource cts = new CancellationTokenSource();
        private Task _calcTask;
        protected override void OnPlay(object obj = null)
        {
            if (IsPlaying)
            {
                IsPlaying = false;
                cts.Cancel();
                _calcTask.Wait();
            }
            else
            {
                IsPlaying = true;
                var token = cts.Token;
                _calcTask = Task.Run(() =>
                {
                    while (Step < _config.StepCount && !token.IsCancellationRequested)
                    {
                        Step++;
                        var t = Application.Current.Dispatcher.InvokeAsync(delegate
                        {
                            Draw(Step);
                        });
                        t.Wait();
                        Task.Delay(50).Wait();
                    }
                }, token);
            }
        }

        protected override void OnRestart(object obj = null)
        {
            if (IsPlaying)
                OnPlay();
            Step = 0;
            Draw(Step, null);
        }
        
        private void Draw(int n, bool? forward = true)
        {
            double actualWidth = 900;
            double actualHeight = 900;
            int pixelSizeW = (int)(actualWidth / _config.Width);
            int pixelSizeH = (int)(actualHeight / _config.Height);
            WriteableBitmap writeableBitmap;
            if (CurrentBitmap == null)
            {
                writeableBitmap = new WriteableBitmap(_config.Width * pixelSizeW, _config.Height * pixelSizeH, 96,
                    96, PixelFormats.Bgr32, null);
            }
            else
            {
                writeableBitmap = CurrentBitmap;
            }
            try
            {
                writeableBitmap.Lock();
                for (int h = 0; h < _config.Height; h++)
                    for (var k = 0; k < _config.Width; k++)
                    {
                        if (CurrentBitmap != null && forward.HasValue && _colors[_caList[h, k, n]] == _colors[_caList[h, k, forward.Value ? n - 1 : n + 1]])
                            continue;

                        unsafe
                        {
                            for (int i = 0; i < pixelSizeH; i++)
                            {
                                for (int j = 0; j < pixelSizeW; j++)
                                {
                                    // Get a pointer to the back buffer.
                                    IntPtr pBackBuffer = writeableBitmap.BackBuffer;

                                    // Find the address of the pixel to draw.
                                    pBackBuffer += (h * pixelSizeH + i) * writeableBitmap.BackBufferStride;
                                    pBackBuffer += (k * pixelSizeW + j) * 4;

                                    // Compute the pixel's color.
                                    var colorData = _colors[_caList[h, k, n]].R << 16; // R
                                    colorData |= _colors[_caList[h, k, n]].G << 8; // G
                                    colorData |= _colors[_caList[h, k, n]].B << 0; // B

                                    // Assign the color data to the pixel.
                                    *(int*)pBackBuffer = colorData;
                                }
                            }
                        }
                    }
                // Specify the area of the bitmap that changed.
                writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, _config.Width * pixelSizeW, _config.Height * pixelSizeH));

            }
            finally
            {
                writeableBitmap.Unlock();
            }

            CurrentBitmap = writeableBitmap;
        }
    }
}