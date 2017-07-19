using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImageLabelingTool_pre.Models
{
    public interface ITiffImage
    {
        string BaseFileFullPath { get; set; }
        BitmapImage DisplayImage { get; }
        WriteableBitmap LabelImageData { get; }
        int ClopSize { get; set; }

        Boolean OpenNewImageFile();
        Boolean OpenAttributeFile();
        Boolean SaveAttributeFile();
        Boolean ClopImageFile();
    }
}
