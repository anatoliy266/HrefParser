using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace HrefParser
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class MainWindowViewModel() : BaseViewModel
    {
        public string Href
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(Href));
            }
        }
        public int Progress
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(Progress));
            }
        }

        public ObservableCollection<HrefDataModel> Data { get; set; } = new ObservableCollection<HrefDataModel>();

        public ICommand MyButtonClick => field ??= new RelayCommand(Click);


        private void Click()
        {
            System.Windows.MessageBox.Show($"{Href}");
        }
    }
}
