using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;

namespace ImageLabelingTool_pre.Models
{
    public interface ITiffImage
    {
        string BaseFileFullPath { get; set; }
        BitmapImage DisplayImage { get; }
        WriteableBitmap LabelImageData { get; }
        int ClopSize { get; set; }

        Boolean OpenNewImageFile(ref string message);
        Boolean OpenAttributeFile(ref string message, ref LabelAttributes attr);
        Boolean SaveImageFile(LabelAttributes labelAttributes);
        Boolean ClopImageFile();
        Boolean LabelingImage(Int32Rect area, byte label, Color disp);
    }
}
