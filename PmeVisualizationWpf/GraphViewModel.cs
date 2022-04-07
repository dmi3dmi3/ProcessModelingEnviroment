using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using OxyPlot;
using PmeVisualizationWpf.Properties;

namespace PmeVisualizationWpf
{
    public class GraphViewModel : INotifyPropertyChanged
    {
        private IList<DataPoint> _values;
        private string _name;
        private Color _color;

        public IList<DataPoint> Values
        {
            get => _values;
            set
            {
                if (Equals(value, _values)) return;
                _values = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public Color Color
        {
            get => _color;
            set
            {
                if (value.Equals(_color)) return;
                _color = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}