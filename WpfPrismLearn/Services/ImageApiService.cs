using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WpfPrismLearn.Models;
using WpfPrismLearn.Services;

namespace WpfPrismLearn.Services
{
    public class ImageApiService : IImageApiService
    {
        private readonly HttpClient _httpClient;

        // HttpClientをDIで受け取るコンストラクタ
        public ImageApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ImageItem>> GetImagesAsync()
        {
            try {
                // 1. APIリクエスト送信 (Picsumから10件取得)
                var response = await _httpClient.GetAsync("https://picsum.photos/v2/list?limit=10");

                // エラーなら例外を投げる
                response.EnsureSuccessStatusCode();

                // 2. JSON文字列として読み込み
                var json = await response.Content.ReadAsStringAsync();

                // 3. JSONをPicsumImageクラスに変換
                var apiData = JsonSerializer.Deserialize<List<PicsumImage>>(json);

                // 4. アプリ内で使う Model (ImageItem) に変換して返す
                // Picsumの画像は巨大なので、URL末尾を加工してリサイズ版を取得するようにします
                return apiData.Select(x => new ImageItem
                {
                    Title = x.Author,
                    // download_url の形式: https://picsum.photos/id/{id}/{width}/{height}
                    // ここでは幅300px, 高さ200pxを指定
                    ImageUrl = $"https://picsum.photos/id/{x.Id}/300/200",
                    IsSelected = false
                }).ToList();
            }
            catch (Exception ex)
            {
                // エラー時はログを出力するか、空リストを返す等のハンドリングを行います
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");
                return new List<ImageItem>();
            }
}
    }
}
