using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using System.Runtime.Serialization;

namespace ImageLabelingTool_pre.Models
{
    [Serializable]
    public class LabelAttribute : ISerializable
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

        public LabelAttribute(string name, byte pixel, string rgb)
        {
            _Name = name;
            _Pixel = pixel;
            _RGB = rgb;
        }

        public LabelAttribute(SerializationInfo info, StreamingContext context)
        {
            _Name = info.GetString("Name");
            _Pixel = info.GetByte("Pixel");
            _RGB = info.GetString("RGB");
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", this._Name);
            info.AddValue("Pixel", this._Pixel);
            info.AddValue("RGB", this._RGB);
        }
    }
}
