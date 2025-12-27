using System.Reactive.Disposables;
using WpfPrismLearn.Events;
using WpfPrismLearn.Models;

namespace WpfPrismLearn.ViewModels
{
    internal class DetailViewModel : BindableBase, INavigationAware, IDestructible
    {
        private readonly IEventAggregator _eventAggregator;

        private CompositeDisposable _disposables = new CompositeDisposable();

        private ImageItem _selectedImage;
        public ImageItem SelectedImage
        {
            get { return _selectedImage; }
            set { SetProperty(ref _selectedImage, value); }
        }
        
        public DetailViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            var token = _eventAggregator.GetEvent<ImageSelectedEvent>().Subscribe(OnImageSelected);

            // トークンを使用して購読解除を行う
            _disposables.Add(Disposable.Create(() =>
            {
                _eventAggregator.GetEvent<ImageSelectedEvent>().Unsubscribe(token);

                System.Diagnostics.Debug.WriteLine("DetailViewModel: イベント購読を解除しました");
            }));
        }

        private void OnImageSelected(ImageItem image)
        {
            SelectedImage = image;
            System.Diagnostics.Debug.WriteLine($"DetailViewModel: 画像 {image.Title} を受信しました");
        }

        // --- INavigationAware の実装 (今回は使いませんが必須なので記述) ---
        public bool IsNavigationTarget(NavigationContext navigationContext) => true; // インスタンスを使い回す
        public void OnNavigatedFrom(NavigationContext navigationContext) { }
        public void OnNavigatedTo(NavigationContext navigationContext) { }

        // --- IDestructible (Viewが消える時に呼ばれる) ---
        public void Destroy()
        {
            // ここでゴミ箱を破棄すると、登録しておいた Unsubscribe も自動実行される
            _disposables.Dispose();
            System.Diagnostics.Debug.WriteLine("DetailViewModel: 破棄されました (Destroy)");
        }
    }
}
