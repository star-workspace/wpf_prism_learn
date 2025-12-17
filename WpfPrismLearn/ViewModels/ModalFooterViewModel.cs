using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using WpfPrismLearn.Events;
using WpfPrismLearn.Models;

namespace WpfPrismLearn.ViewModels
{
    public class ModalFooterViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        private ImageItem _currentTargetImage;

        public DelegateCommand ConfirmCommand { get; }
        public DelegateCommand CloseCommand { get; }

        public ModalFooterViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            // 画像選択のイベントを購読して、現在のターゲット画像を更新
            _eventAggregator.GetEvent<ImageSelectedEvent>().Subscribe(img => _currentTargetImage = img);

            ConfirmCommand = new DelegateCommand(OnConfirm);
            CloseCommand = new DelegateCommand(OnClose);
        }

        private void OnConfirm()
        {
            if (_currentTargetImage != null)
            {
                // 確定イベントを発行
                _eventAggregator.GetEvent<ImageConfirmedEvent>().Publish(_currentTargetImage);
            }
        }

        private void OnClose()
        {
            // モーダルを閉じるイベントを発行
            _eventAggregator.GetEvent<CloseModalEvent>().Publish();
        }
    }
}
