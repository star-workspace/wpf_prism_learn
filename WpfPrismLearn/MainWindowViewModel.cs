using Prism.Commands;
using Prism.Mvvm;
using Prism.Events;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WpfPrismLearn.Models;
using WpfPrismLearn.Services;
using WpfPrismLearn.Events;

namespace WpfPrismLearn
{
    class MainWindowViewModel : BindableBase
    {
        private readonly IGreetingService _greetingService;
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;

        private bool _isDetailMode = false;

        private string _inputName = string.Empty;
        public string InputName
        {
            get { return _inputName; }
            set { SetProperty(ref _inputName, value); }
        }

        private string _resultMessage = string.Empty;
        public string ResultMessage
        {
            get { return _resultMessage; }
            set { SetProperty(ref _resultMessage, value); }
        }

        private bool _isModalVisible;
        public bool IsModalVisible
        {
            get { return _isModalVisible; }
            set { SetProperty(ref _isModalVisible, value); }
        }

        private string _modalMessage = string.Empty;
        public string ModalMessage
        {
            get { return _modalMessage; }
            set { SetProperty(ref _modalMessage, value); }
        }

        private ImageItem _confirmedImage;
        public ImageItem ConfirmedImage
        {
            get { return _confirmedImage; }
            set { SetProperty(ref _confirmedImage, value); }
        }

        public DelegateCommand GreetCommand { get; }
        public DelegateCommand<ImageItem> SelectImageCommand { get; }
        public DelegateCommand CloseModalCommand { get; }
        public DelegateCommand SwitchContentCommand { get; }

        public ObservableCollection<ImageItem> ImageItems { get; } = new ObservableCollection<ImageItem>();

        public MainWindowViewModel(IGreetingService greetingService, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _greetingService = greetingService;
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<ImageConfirmedEvent>().Subscribe(image =>
            {
                ConfirmedImage = image;
                // 確定したらモーダルを閉じるならここに追加
                IsModalVisible = false;
            });

            // FooterViewModelから「閉じて」と言われたらここが動く
            _eventAggregator.GetEvent<CloseModalEvent>().Subscribe(() =>
            {
                IsModalVisible = false;
            });

            GreetCommand = new DelegateCommand(ExecuteGreet);

            SelectImageCommand = new DelegateCommand<ImageItem>(item =>
            {
                // 1. 一旦、リスト内のすべての画像の選択を解除する
                foreach (var image in ImageItems)
                {
                    image.IsSelected = false;
                }

                // 2. クリックされた画像だけを選択状態にする
                item.IsSelected = true;

                IsModalVisible = true;
                ModalMessage = "詳細を読み込んでいます…";

                _regionManager.RequestNavigate("ModalContentRegion", "DetailView", result =>
                {
                    // ナビゲーション完了後にイベントを発行して、DetailViewModelに選択された画像を通知
                    if (result.Success)
                    {
                        _eventAggregator.GetEvent<ImageSelectedEvent>().Publish(item);
                    }
                });
            });

            CloseModalCommand = new DelegateCommand(() =>
            {
                IsModalVisible = false;
            });

            SwitchContentCommand = new DelegateCommand(() =>
            {
                // フラグを反転
                _isDetailMode = !_isDetailMode;

                // 表示するView名を決定 (App.xaml.csで登録したクラス名)
                string viewName = _isDetailMode ? "DetailView" : "MessageView";

                // 指定したRegionに、指定したViewへ遷移(Navigate)するよう依頼
                _regionManager.RequestNavigate("ModalContentRegion", viewName);
            });

            LoadSampleImages();
            _eventAggregator = eventAggregator;
        }

        private void LoadSampleImages()
        {
            ImageItems.Add(new ImageItem { Title = "Mountain", ImageUrl = "https://picsum.photos/id/10/200/150" });
            ImageItems.Add(new ImageItem { Title = "River", ImageUrl = "https://picsum.photos/id/11/200/150" });
            ImageItems.Add(new ImageItem { Title = "Sea", ImageUrl = "https://picsum.photos/id/12/200/150" });
            ImageItems.Add(new ImageItem { Title = "Forest", ImageUrl = "https://picsum.photos/id/13/200/150" });
        }

        private void ExecuteGreet()
        {
            if (string.IsNullOrWhiteSpace(InputName))
            {
                ModalMessage = "名前を入力してください！";
            }
            else
            {
                ModalMessage = _greetingService.GetMessage(InputName);

                ImageItems.Add(new ImageItem
                {
                    Title = $"New Item {ImageItems.Count + 1}",
                    ImageUrl = $"https://picsum.photos/seed/{20 + ImageItems.Count}/200/150"
                });
            }
            IsModalVisible = true; // ★これで表示される
        }
    }
}
