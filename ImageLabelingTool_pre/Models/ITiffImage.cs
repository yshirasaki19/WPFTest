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
        string FileFullPath { get; set; }
        BitmapImage BitmapImage { get; }
    }
}
