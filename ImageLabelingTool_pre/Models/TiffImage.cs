using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Win32;

namespace ImageLabelingTool_pre.Models
{
    public class TiffImage : AbstractTiffImage
    {
        private const String LABEL_FILE_EXT = "_label.tif";

        #region フィールド
        // 元画像ファイルパス(フルパス)
        protected string _BaseFileFullPath;
        public override string BaseFileFullPath
        {
            get { return _BaseFileFullPath; }
            set { _BaseFileFullPath = value; }
        }
 
        // 表示用画像データ
        protected BitmapImage _DisplayImage;
        public override BitmapImage DisplayImage
        {
            get { return _DisplayImage; }
        }

        // 編集用ラベル画像データ
        protected WriteableBitmap _LabelImageData;
        public override WriteableBitmap LabelImageData
        {
            get { return _LabelImageData; }
        }

        // クロップサイズ
        protected int _ClopSize;
        public override int ClopSize
        {
            get { return _ClopSize; }
            set { _ClopSize = value; }
        }
        #endregion

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TiffImage()
        {
            _BaseFileFullPath = null;
            _LabelImageData = null;
            _DisplayImage = null;
        }

        /// <summary>
        /// 新規に処理対象となる画像を開く
        /// </summary>
        /// <returns></returns>
        public override Boolean OpenNewImageFile()
        {
            // 保存ダイアログを表示する。
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "開くファイルを選択してください";
            dialog.Filter = "Tiff Bitmap画像|*.tiff;*.tif;*.bmp";

            if (dialog.ShowDialog() == true)
            {
                _BaseFileFullPath = dialog.FileName;
                _DisplayImage = new BitmapImage(new Uri(_BaseFileFullPath));
                _LabelImageData = new WriteableBitmap(new BitmapImage(new Uri(_BaseFileFullPath)));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 既存の設定ファイルを開く
        /// </summary>
        /// <returns></returns>
        public override Boolean OpenAttributeFile()
        {
            // 選択ダイアログを表示する。
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "開くファイルを選択してください";
            dialog.Filter = "設定ファイル|*.xml;";

            if (dialog.ShowDialog() == true)
            {
                // 設定ファイルから各情報を作成
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 設定ファイルを保存する
        /// </summary>
        /// <returns></returns>
        public override Boolean SaveAttributeFile()
        {
            // 選択ダイアログを表示する。
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "保存先のファイルを選択してください";
            dialog.Filter = "設定ファイル|*.xml;";

            if (dialog.ShowDialog() == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 元画像、ラベル画像をクロップする
        /// </summary>
        /// <returns></returns>
        public override Boolean ClopImageFile()
        {
            // 選択ダイアログを表示する。
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "保存先のフォルダを選択してください";

            if (dialog.ShowDialog() == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
