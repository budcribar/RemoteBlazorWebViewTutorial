using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteBlazorWebViewTutorial.WpfApp
{
    public class ViewModel : INotifyPropertyChanged
    {
        private bool hyperLinkVisible = false;

        public bool HyperLinkVisible
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
