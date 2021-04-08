
using System;
using System.Threading.Tasks;

namespace CrsCommon.Controls.VideoPlayerLibrary
{
    public interface IVideoPicker
    {
        Task<string> GetVideoFileAsync();
    }
}