using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeePassW10.Common
{
    public class FilteredObservableCollection<T> : ObservableCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private ObservableCollection<T> _dataList;

        public new IEnumerator GetEnumerator()
        {
            return _dataList.GetEnumerator();
        }


        public FilteredObservableCollection(ObservableCollection<T> source)
        {
            _dataList = source;
        }

    }

}
