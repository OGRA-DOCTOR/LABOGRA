// الإصدار 1: BaseViewModel.cs
// الوصف: فئة أساسية لتطبيق نمط MVVM، توفر آلية الإشعار بتغيير الخصائص.
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic; // Required for EqualityComparer

namespace LABOGRA.Core
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}