using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using System.Windows.Input;

using Prism.Commands;
using Prism.Mvvm;
using Prism.Events;
using Prism.Navigation;

using WpfPrismLearn.Models;
using WpfPrismLearn.Services;
using WpfPrismLearn.Events;

namespace WpfPrismLearn
{
    class MainWindowViewModel : BindableBase
    {
        private readonly IGreetingService _greetingService;
        private readonly IThemeService _themeService;

        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;

        private readonly IImageApiService _imageApiService;

        //  全ての購読をまとめるゴミ箱
        private CompositeDisposable _disposables = new CompositeDisposable();

        // スライドショー用の使い捨て容器（Start/Stop切り替え用）
        private IDisposable? _slideShowSubscription;

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

        private bool _isConfirmedImageActive;
        public bool IsConfirmedImageActive
        {
            get { return _isConfirmedImageActive; }
            set { SetProperty(ref _isConfirmedImageActive, value); }
        }

        // ダークモードかどうか
        private bool _isDarkMode;
        public bool IsDarkMode
        {
            get { return _isDarkMode; }
            set
            {
                if (SetProperty(ref _isDarkMode, value))
                {
                    // プロパティが変わったら即座にテーマを変更
                    _themeService.SetTheme(_isDarkMode);
                }
            }
        }

        // 自動再生中かどうか
        private bool _isAutoPlay;
        public bool IsAutoPlay
        {
            get { return _isAutoPlay; }
            set
            {
                if (SetProperty(ref _isAutoPlay, value))
                {
                    if (_isAutoPlay) StartSlideShow();
                    else StopSlideShow();
                }
            }
        }

        // 通信中かどうか
        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        public DelegateCommand GreetCommand { get; }
        public DelegateCommand<ImageItem> SelectImageCommand { get; }
        public DelegateCommand CloseModalCommand { get; }
        public DelegateCommand SwitchContentCommand { get; }
        public DelegateCommand ToggleConfirmedImageCommand { get; }
        public DelegateCommand ReloadCommand { get; }

        public ObservableCollection<ImageItem> ImageItems { get; } = new ObservableCollection<ImageItem>();

        public MainWindowViewModel(
            IGreetingService greetingService,
            IThemeService themeService,
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            IImageApiService imageApiService)
        {
            _greetingService = greetingService;
            _themeService = themeService;
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            _imageApiService = imageApiService;

            _eventAggregator.GetEvent<ImageConfirmedEvent>().Subscribe(image =>
            {
                ConfirmedImage = image;
                // 確定したらモーダルを閉じるならここに追加
                IsModalVisible = false;

                // 新しい画像が反映されたら、赤枠は一旦リセット（解除）しておく
                IsConfirmedImageActive = false;
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

            ToggleConfirmedImageCommand = new DelegateCommand(() =>
            {
                // true <-> false を反転させる (トグル動作)
                IsConfirmedImageActive = !IsConfirmedImageActive;
            });

            // リロードコマンド
            ReloadCommand = new DelegateCommand(
                executeMethod: () => LoadImagesAsync(),      // 実行する処理
                canExecuteMethod: () => !IsBusy              // 実行可能かどうかの判定 (通信中は押せない)
            )
            .ObservesProperty(() => IsBusy); // ★重要: IsBusyプロパティが変わるたびに判定を再チェックする

            LoadImagesAsync();
            _eventAggregator = eventAggregator;
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

        private void StartSlideShow()
        {
            StopSlideShow(); // 念のため既存のタイマーがあれば消す

            // ★Rx: 2秒ごとにイベントを発火
            _slideShowSubscription = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(2))
                .ObserveOn(SynchronizationContext.Current) // ★注意点: UIスレッドに戻す
                .Subscribe(_ =>
                {
                    SelectNextImage();
                });

            // ★重要: 作成した購読(Subscription)をゴミ箱に入れる
            _disposables.Add(_slideShowSubscription);
        }

        private void StopSlideShow()
        {
            // 個別のタイマーを停止（破棄）
            if (_slideShowSubscription != null)
            {
                _slideShowSubscription.Dispose();
                _disposables.Remove(_slideShowSubscription); // ゴミ箱からもリスト削除しておく
                _slideShowSubscription = null;
            }
        }

        private void SelectNextImage()
        {
            if (ImageItems.Count == 0) return;

            // 現在選択されている画像のインデックスを探す
            var currentIndex = -1;
            for (int i = 0; i < ImageItems.Count; i++)
            {
                if (ImageItems[i].IsSelected)
                {
                    currentIndex = i;
                    break;
                }
            }

            // 次の画像へ（最後まで行ったら最初に戻る）
            var nextIndex = (currentIndex + 1) % ImageItems.Count;

            // 既存のコマンドを再利用して選択処理を実行
            SelectImageCommand.Execute(ImageItems[nextIndex]);
        }

        // ---------------------------------------------------------
        // ★ IDestructible の実装 (画面が閉じられる時に呼ばれる)
        // ---------------------------------------------------------
        public void Destroy()
        {
            // ★ここが最重要ポイント！
            // 画面が破棄されるとき、まとめ役の _disposables を破棄します。
            // これにより、中に入っているタイマー(_slideShowSubscription)も連鎖して止まります。
            // これを忘れると、画面を閉じても裏で SelectNextImage() が動き続け、エラーになります。
            _disposables.Dispose();
        }

        // 非同期メソッド
        private async void LoadImagesAsync()
        {
            // ローディング開始
            IsBusy = true;
            ImageItems.Clear();

            // API呼び出し
            var images = await _imageApiService.GetImagesAsync();

            foreach (var image in images)
            {
                ImageItems.Add(image);
            }

            // ローディング終了
            IsBusy = false;
        }
    }
}
