using System.Collections.Generic;
using System.Threading.Tasks;
using WpfPrismLearn.Models;

namespace WpfPrismLearn.Services
{
    public interface IImageApiService
    {
        Task<List<ImageItem>> GetImagesAsync();
    }
}
