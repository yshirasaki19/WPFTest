using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;

namespace ImageLabelingTool_pre.Models
{
    public class LabelAttribute : NotificationObject
    {
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private byte _Pixel;
        public byte Pixel
        {
            get { return _Pixel; }
            set { _Pixel = value; }
        }

        public string _RGB;
        public string RGB
        {
            get { return _RGB; }
            set { _RGB = value; }
        }


        public LabelAttribute()
        {
        }

        public LabelAttribute(string name, byte pixel, string rgb)
        {
            _Name = name;
            _Pixel = pixel;
            _RGB = rgb;
        }
    }
}
