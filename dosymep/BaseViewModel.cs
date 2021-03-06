using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

namespace dosymep.WPF.ViewModels {
    internal class BaseViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Предоставляет доступ к логгеру платформы.
        /// </summary>
        protected ILoggerService LoggerService => GetPlatformService<ILoggerService>();

        protected void RaiseAndSetIfChanged<TRet>(ref TRet backingField, TRet newValue,
            [CallerMemberName] string propertyName = null) {
            if(!EqualityComparer<TRet>.Default.Equals(backingField, newValue)) {
                backingField = newValue;
                RaisePropertyChanged(propertyName);
            }
        }

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }

        protected virtual void RaisePropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}