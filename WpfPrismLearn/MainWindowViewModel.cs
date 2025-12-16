using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WpfPrismLearn.Models;
using WpfPrismLearn.Services;

namespace WpfPrismLearn
{
    class MainWindowViewModel : BindableBase
    {
        private readonly IGreetingService _greetingService;

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

        public DelegateCommand GreetCommand { get; }
        public DelegateCommand<ImageItem> SelectImageCommand { get; }

        public ObservableCollection<ImageItem> ImageItems { get; } = new ObservableCollection<ImageItem>();

        public MainWindowViewModel(IGreetingService greetingService)
        {
            _greetingService = greetingService;
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
            });

            LoadSampleImages();
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
                ResultMessage = "Please enter your name.";
            }
            else
            {
                ResultMessage = _greetingService.GetMessage(InputName);

                ImageItems.Add(new ImageItem
                {
                    Title = $"New Item {ImageItems.Count + 1}",
                    ImageUrl = $"https://picsum.photos/seed/{20 + ImageItems.Count}/200/150"
                });
            }
        }
    }
}
