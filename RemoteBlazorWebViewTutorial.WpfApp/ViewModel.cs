using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RemoteBlazorWebViewTutorial.WpfApp
{
    public class ViewModel : INotifyPropertyChanged
    {
        private Visibility hyperLinkVisible = Visibility.Hidden;

        public Visibility HyperLinkVisible
        {
            get
            {
                return hyperLinkVisible;
            }

            set
            {
                hyperLinkVisible = value;
                NotifyPropertyChanged("HyperLinkVisible");
            }
        }

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
