using System;
using System.Linq;
using System.Windows;

namespace WpfPrismLearn.Services
{
    class ThemeService : IThemeService
    {
        public void SetTheme(bool isDark)
        {
            // 1. 新しいテーマのURIを決定
            string themeName = isDark ? "DarkTheme" : "LightTheme";
            var newUri = new Uri($"pack://application:,,,/WpfPrismLearn;component/Styles/{themeName}.xaml");

            // 2. Application.Current.Resources.MergedDictionaries を操作
            var dictionaries = Application.Current.Resources.MergedDictionaries;

            // ★注意点: 全部Clearしてはいけません！
            // MaterialDesignなどのライブラリも含まれているため、
            // 「自分のアプリのテーマファイル」だけを探して削除する必要があります。
            var existingTheme = dictionaries.FirstOrDefault(d =>
                d.Source != null && d.Source.OriginalString.Contains("Styles/")
            );

            if (existingTheme != null)
            {
                dictionaries.Remove(existingTheme);
            }

            // 3. 新しいテーマを追加
            dictionaries.Add(new ResourceDictionary { Source = newUri });
        }
    }
}
