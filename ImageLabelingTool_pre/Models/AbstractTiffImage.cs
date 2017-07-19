using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using System.Windows.Media.Imaging;


namespace ImageLabelingTool_pre.Models
{
    public abstract class AbstractTiffImage : NotificationObject , ITiffImage
    {
        public abstract string BaseFileFullPath { get; set; }
        public abstract BitmapImage DisplayImage  { get; }
        public abstract WriteableBitmap LabelImageData { get; }
        public abstract int ClopSize { get; set; }

        public abstract Boolean OpenNewImageFile();
        public abstract Boolean OpenAttributeFile();
        public abstract Boolean SaveAttributeFile();
        public abstract Boolean ClopImageFile();
    }
}
