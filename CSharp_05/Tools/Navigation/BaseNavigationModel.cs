using System.Collections.Generic;

namespace CSharp_05.Tools.Navigation
{
    internal abstract class BaseNavigationModel : INavigationModel
    {
        protected BaseNavigationModel(IContentOwner contentOwner)
        {
            ContentOwner = contentOwner;
            ViewDictionary = new Dictionary<ViewType, INavigatable>();
        }

        protected IContentOwner ContentOwner { get; }

        protected Dictionary<ViewType, INavigatable> ViewDictionary { get; }

        public void Navigate(ViewType viewType)
        {
            if (!ViewDictionary.ContainsKey(viewType))
                InitializeView(viewType);
            ContentOwner.ContentControl.Content = ViewDictionary[viewType];
        }

        protected abstract void InitializeView(ViewType viewType);

    }
}