
using System;
using System.Threading.Tasks;

namespace Common.Controls.VideoPlayerLibrary
{
    public interface IVideoPicker
    {
        Task<string> GetVideoFileAsync();
    }
}