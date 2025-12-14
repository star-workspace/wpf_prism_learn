using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Input;
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

        public MainWindowViewModel(IGreetingService greetingService)
        {
            _greetingService = greetingService;
            GreetCommand = new DelegateCommand(ExecuteGreet);
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
            }
        }
    }
}
