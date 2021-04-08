using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Common.UI.Interfaces
{
    public interface IPopupMenu
    {
        string folderPath { get; }
        Task<int> LaunchMenu(List<string> menuItems, Xamarin.Forms.View parent = null);
        Task<string> LaunchInputTextMenu(string title);
        Task<Stream> LaunchFilePicker(bool isImage = true);
        void ShowContentPopup(Xamarin.Forms.View content, int width, int height, Xamarin.Forms.View parent);
        void CancelPopup();
    }
}
