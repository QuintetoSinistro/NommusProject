using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NommusProject.Utils
{
    public static class ImageHelper
    {
        public static ImageSource CarregarFoto(string caminho, string defaultPath = "/Views/Images/user.png")
        {
            try
            {
                if (!string.IsNullOrEmpty(caminho) && File.Exists(caminho))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(caminho, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch { }

            try
            {
                return new BitmapImage(new Uri(defaultPath, UriKind.Relative));
            }
            catch
            {
                return null;
            }
        }
    }
}