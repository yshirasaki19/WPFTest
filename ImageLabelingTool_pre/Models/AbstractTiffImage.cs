using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;

namespace ImageLabelingTool_pre.Models
{
    public abstract class AbstractTiffImage : NotificationObject, ITiffImage
    {
        public abstract string BaseFileFullPath { get; set; }
        public abstract BitmapImage DisplayImage { get; }
        public abstract WriteableBitmap LabelImageData { get; }
        public abstract int ClopSize { get; set; }

        public abstract Boolean OpenNewImageFile(ref string message);
        public abstract Boolean OpenAttributeFile(ref string message, ref LabelAttributes attr);
        public abstract Boolean SaveImageFile(LabelAttributes labelAttributes);
        public abstract Boolean ClopImageFile();

        public abstract Boolean LabelingImage(Int32Rect area, byte label, Color disp);
    }
}