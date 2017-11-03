using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Adapt.Presentation.Controls
{
    public class CollectionInformation : INotifyPropertyChanged
    {
        #region Fields
        private IEnumerable _Collection;
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Public Properties
        public string CollectionName { get; set; }
        public IEnumerable Collection
        {
            get { return _Collection; }
            set
            {
                _Collection = value;

                var notifyCollectionChanged = value as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                {
                    notifyCollectionChanged.CollectionChanged += CollectionInformation_CollectionChanged;
                }

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Collection"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Count"));
                }
            }
        }
        public int? Count
        {
            get
            {
                var countProperty = Collection?.GetType().GetProperty("Count");
                return (int?)countProperty?.GetValue(Collection, null);
            }
        }

        #endregion

        #region Constructor
        public CollectionInformation(string collectionName, IEnumerable collection)
        {
            CollectionName = collectionName;
            Collection = collection;
        }
        #endregion

        #region Event Handlers
        private void CollectionInformation_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Collection"));
                PropertyChanged(this, new PropertyChangedEventArgs("Count"));
            }
        }
        #endregion
    }
}