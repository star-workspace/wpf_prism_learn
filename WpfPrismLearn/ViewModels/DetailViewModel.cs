using WpfPrismLearn.Events;
using WpfPrismLearn.Models;

namespace WpfPrismLearn.ViewModels
{
    internal class DetailViewModel : BindableBase, INavigationAware
    {
        private readonly IEventAggregator _eventAggregator;

        private ImageItem _selectedImage;
        public ImageItem SelectedImage
        {
            get { return _selectedImage; }
            set { SetProperty(ref _selectedImage, value); }
        }
        
        public DetailViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<ImageSelectedEvent>().Subscribe(OnImageSelected);
        }

        private void OnImageSelected(ImageItem image)
        {
            SelectedImage = image;
        }

        // --- INavigationAware の実装 (今回は使いませんが必須なので記述) ---
        public bool IsNavigationTarget(NavigationContext navigationContext) => true; // インスタンスを使い回す
        public void OnNavigatedFrom(NavigationContext navigationContext) { }
        public void OnNavigatedTo(NavigationContext navigationContext) { }
    }
}
