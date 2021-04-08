using System.Threading.Tasks;

namespace Common.UI.Controls.VideoPlayerLibrary
{
    public interface IVideoPicker
    {
        Task<string> GetVideoFileAsync();
    }
}