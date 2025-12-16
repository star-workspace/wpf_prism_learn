using System.Windows;
using Prism.DryIoc;
using Prism.Ioc;
using WpfPrismLearn.Services;
using WpfPrismLearn.Views;

namespace WpfPrismLearn
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        // 1. メインウィンドウを作成して返すメソッド
        protected override Window CreateShell()
        {
            // 最初に表示する画面を返す
            return Container.Resolve<MainWindow>();
        }

        // 2. DIコンテナにクラスなどを登録するメソッド
        // (今は空でOKですが、将来Serviceなどをここに登録します)
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IGreetingService, GreetingService>();
        }

        // 3. アプリケーション起動時の初期化処理を行うメソッド
        protected override void OnInitialized()
        {
            base.OnInitialized();

            var regionManager = Container.Resolve<IRegionManager>();

            regionManager.RegisterViewWithRegion("HeaderRegion", typeof(HeaderView));

            // モーダル内のヘッダーとフッター
            regionManager.RegisterViewWithRegion("ModalHeaderRegion", typeof(ModalHeader));
            regionManager.RegisterViewWithRegion("ModalFooterRegion", typeof(ModalFooter));
        }
    }

}
