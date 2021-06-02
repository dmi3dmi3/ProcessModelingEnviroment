using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Shapes;
using PmeVisualizationWpf.Annotations;

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
        protected virtual void OnPathSelect(object obj = null) { }
        protected virtual void OnNext(object obj = null) { }
        protected virtual void OnPrevious(object obj = null) { }
        protected virtual void OnPlay(object obj = null) { }

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

        public ObservableCollection<Shape> CanvasItemsSource
        {
            get => _canvasItemsSource;
            set
            {
                if (Equals(value, _canvasItemsSource)) return;
                _canvasItemsSource = value;
                OnPropertyChanged();
            }
        }


        public ICommand SelectPathCommand => _selectPathCommand ?? (_selectPathCommand = new CommandWrapper(OnPathSelect));
        public ICommand NextCommand => _nextCommand ?? (_nextCommand = new CommandWrapper(OnNext));
        public ICommand PreviousCommand => _previousCommand ?? (_previousCommand = new CommandWrapper(OnPrevious));
        public ICommand PlayCommand => _playCommand ?? (_playCommand = new CommandWrapper(OnPlay));

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}