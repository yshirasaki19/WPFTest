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
        // 元画像ファイルパス(フルパス)
        protected string _FileFullPath;
        public string FileFullPath
        {
            get { return _FileFullPath; }
            set {
                _FileFullPath = value;
                // ファイルパスを設定した際にBitmapImageを作成
                _BitmapImage = new BitmapImage(new Uri(_FileFullPath));
            }
        }
         
        // 画像データ(BitmapImage)
        protected BitmapImage _BitmapImage;
        public BitmapImage BitmapImage
        {
            get { return _BitmapImage; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AbstractTiffImage()
        {
            _FileFullPath = null;
            _BitmapImage = null;
        }
    }
}
