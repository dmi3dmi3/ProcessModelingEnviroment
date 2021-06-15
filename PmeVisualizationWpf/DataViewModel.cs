using System.Collections.Generic;
using PmeVisualizationWpf.Annotations;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Shapes;

namespace PmeVisualizationWpf
{
    public class DataViewModel : INotifyPropertyChanged
    {
        private string _projectPath;
        private ICommand _selectPathCommand;
        private ICommand _nextCommand;
        private ICommand _previousCommand;
        private ICommand _playCommand;
        private ObservableCollection<Shape> _canvasItemsSource;
        private int _step;
        private ObservableCollection<GraphViewModel> _graphsItemSource;
        private bool _isPlaying;
        private ICommand _restartCommand;
        private bool _isNotProcessing;

        protected virtual void OnPathSelect(object obj = null) { }
        protected virtual void OnNext(object obj = null) { }
        protected virtual void OnPrevious(object obj = null) { }
        protected virtual void OnPlay(object obj = null) { }
        protected virtual void OnRestart(object obj = null) { }

        public string PlayButtonText => IsPlaying ? "Stop" : "Play";
        public double CanvasWidth { get; set; }
        public double CanvasHeight { get; set; }
        public int Step
        {
            get => _step;
            set
            {
                if (value == _step) return;
                _step = value;
                OnPropertyChanged();
            }
        }
        public string ProjectPath
        {
            get => _projectPath;
            set
            {
                if (value == _projectPath)
                    return;
                _projectPath = value;
                OnPropertyChanged();
            }
        }
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (value == _isPlaying) return;
                _isPlaying = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PlayButtonText));
            }
        }

        public bool IsNotProcessing
        {
            get => _isNotProcessing;
            set
            {
                if (value == _isNotProcessing) return;
                _isNotProcessing = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<GraphViewModel> GraphsItemSource
        {
            get => _graphsItemSource;
            set
            {
                if (Equals(value, _graphsItemSource))
                    return;
                _graphsItemSource = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Shape> CanvasItemsSource
        {
            get => _canvasItemsSource;
            set
            {
                if (Equals(value, _canvasItemsSource))
                    return;
                _canvasItemsSource = value;
                OnPropertyChanged();
            }
        }


        public ICommand SelectPathCommand => _selectPathCommand ?? (_selectPathCommand = new CommandWrapper(OnPathSelect));
        public ICommand NextCommand => _nextCommand ?? (_nextCommand = new CommandWrapper(OnNext));
        public ICommand PreviousCommand => _previousCommand ?? (_previousCommand = new CommandWrapper(OnPrevious));
        public ICommand PlayCommand => _playCommand ?? (_playCommand = new CommandWrapper(OnPlay));
        public ICommand RestartCommand => _restartCommand ?? (_restartCommand = new CommandWrapper(OnRestart));


        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}