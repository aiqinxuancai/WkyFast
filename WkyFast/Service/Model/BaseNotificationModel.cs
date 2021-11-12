using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WkyFast.Service.Model
{
    [AddINotifyPropertyChangedInterface]
    public class BaseNotificationModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public void OnPropertyChanged([CallerMemberName] string PropertyName = "")
        {
            PropertyChangedEventArgs propertyChangedEventArgs = new PropertyChangedEventArgs(PropertyName);
            PropertyChanged(this, propertyChangedEventArgs);
        }
    }
}
