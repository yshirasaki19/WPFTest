using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.Xml;

using ImageLabelingTool_pre.Utility;
using System.Collections.ObjectModel;

namespace ImageLabelingTool_pre.Models
{
    public class TiffImage : AbstractTiffImage
    {
        private const String LABEL_FILE_EXT = "_label.tif";

        #region フィールド
        // 元画像ファイルパス(フルパス)
        private string _BaseFileFullPath;
        public override string BaseFileFullPath
        {
            get { return _BaseFileFullPath; }
            set { _BaseFileFullPath = value; }
        }
 
        // 表示用画像データ
        private BitmapImage _DisplayImage;
        public override BitmapImage DisplayImage
        {
            get { return _DisplayImage; }
        }

        // 編集用ラベル画像データ
        private WriteableBitmap _LabelImageData;
        public override WriteableBitmap LabelImageData
        {
            get { return _LabelImageData; }
        }

        // クロップサイズ
        private int _ClopSize;
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
        public override Boolean OpenNewImageFile(ref string message)
        {
            // 保存ダイアログを表示する。
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "開くファイルを選択してください";
            dialog.Filter = "画像ファイル|*.tif;*.bmp";
            
            if (dialog.ShowDialog() == true)
            {
                _BaseFileFullPath = dialog.FileName;
                _DisplayImage = new BitmapImage(new Uri(_BaseFileFullPath));
                _LabelImageData = new WriteableBitmap(new BitmapImage(new Uri(_BaseFileFullPath)));
                
                // ビット深度のチェック
                if (!(ToolUtility.CheckImagePixelFormat(_DisplayImage)))
                {
                    _BaseFileFullPath = null;
                    _DisplayImage = null;
                    _LabelImageData = null;
                    message = "画像ファイルが1bit または8bitではありません。";
                    return false;
                }

                // ラベル画像の初期化(全エリアの白(=255)でない個所を属性B(=0)に設定)
                Int32Rect area = new Int32Rect(0, 0, _DisplayImage.PixelWidth, _DisplayImage.PixelHeight);
                if (!(ToolUtility.ChangePixelData(ref _LabelImageData, area, 0)))
                {
                    _BaseFileFullPath = null;
                    _DisplayImage = null;
                    _LabelImageData = null;
                    message = "ラベル画像の初期化でエラーが発生しました。";
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 既存の設定ファイルを開く
        /// </summary>
        /// <returns></returns>
        public override Boolean OpenAttributeFile(ref string message, ref LabelAttributes attr)
        {
            string baseFilePath = "";
            string labelFilePath = "";
            ObservableCollection<LabelAttribute> attrFromFile = new ObservableCollection<LabelAttribute>();
            // 選択ダイアログを表示する。
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "開くファイルを選択してください";
            dialog.Filter = "設定ファイル|*.xml";

            if (dialog.ShowDialog() == true)
            {
                // 設定ファイルを読み込む
                XmlDocument xml = new XmlDocument();
                xml.Load(dialog.FileName);

                foreach (XmlElement element in xml.DocumentElement)
                {
                    if (element.Name == "image")
                    {
                        foreach (XmlElement child in element.ChildNodes)
                        {
                            if (child.Name == "srcfile")
                                baseFilePath = child.InnerText;
                            else if(child.Name == "attrfile")
                                labelFilePath = child.InnerText;
                        }
                    }
                    else if (element.Name == "attrList")
                    {
                        foreach (XmlElement child in element.ChildNodes)
                        {
                            if (child.Name == "attr")
                            {
                                string name = child.GetAttribute("name");
                                byte pixel = Byte.Parse(child.GetAttribute("value"));
                                string rgb = child.GetAttribute("color");
                                LabelAttribute labelAttribute = new LabelAttribute(name, pixel, rgb);
                                attrFromFile.Add(labelAttribute);
                            }                            
                        }
                    } 
                }

                // チェックの実施
                // 元画像が存在すること。
                // ラベル画像が存在すること。
                // 画像のサイズチェック
                // 属性設定ファイルの存在チェック

                // データ設定
                _BaseFileFullPath = baseFilePath;
                _DisplayImage = new BitmapImage(new Uri(labelFilePath));
                _LabelImageData = new WriteableBitmap(new BitmapImage(new Uri(labelFilePath)));
                // ToDo: LabelImageから属性値を使って色つきの画像を復元する。

                attr.Attributes = attrFromFile;
            }
            return true;
        }

        /// <summary>
        /// ラベル画像、設定ファイルを保存する
        /// </summary>
        /// <returns></returns>
        public override Boolean SaveImageFile(LabelAttributes labelAttributes)
        {
            string configFileName;
            string labelFileName;

            // ダイアログを表示する。
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "保存先ファイルを選択してください";
            dialog.InitialDirectory = Path.GetDirectoryName(_BaseFileFullPath);
            dialog.FileName = Path.GetFileNameWithoutExtension(_BaseFileFullPath) + LABEL_FILE_EXT;
            dialog.Filter = "画像ファイル|*.tif";

            if (dialog.ShowDialog() == true)
            {
                // ラベル画像を設定ファイルパスに保存する
                labelFileName = dialog.FileName;
                using (FileStream stream = new FileStream(labelFileName, FileMode.Create))
                {
                    TiffBitmapEncoder encoder = new TiffBitmapEncoder();
                    encoder.Compression = TiffCompressOption.Lzw;
                    encoder.Frames.Add(BitmapFrame.Create(_LabelImageData));
                    encoder.Save(stream);
                }

                configFileName = Path.GetDirectoryName(dialog.FileName) + "\\" + Path.GetFileNameWithoutExtension(dialog.FileName) + ".xml";    
                CreateConfigXMLFile(configFileName, labelFileName, labelAttributes);
            }
            return true;
        }

        /// <summary>
        /// 設定ファイルを作成する
        /// </summary>
        private void CreateConfigXMLFile(string configFileName, string labelFileName, LabelAttributes labelAttributes)
        {
            // 設定ファイルXMLの作成
            XmlDocument xml = new XmlDocument();
            XmlDeclaration dec = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = xml.CreateElement("DrawingImageAttribute");
            xml.AppendChild(dec);
            xml.AppendChild(root);

            // imageタグの設定
            XmlElement image = xml.CreateElement("image");
            root.AppendChild(image);

            XmlElement srcfile = xml.CreateElement("srcfile");
            XmlElement attrfile = xml.CreateElement("attrfile");
            srcfile.InnerText = _BaseFileFullPath;
            attrfile.InnerText = labelFileName;
            image.AppendChild(srcfile);
            image.AppendChild(attrfile);

            // attrListタグの設定
            XmlElement attrList = xml.CreateElement("attrList");
            root.AppendChild(attrList);

            foreach (LabelAttribute data in labelAttributes.Attributes)
            {
                XmlElement attr = xml.CreateElement("attr");
                attr.SetAttribute("name", data.Name);
                attr.SetAttribute("value", data.Pixel.ToString());
                attr.SetAttribute("color", data.RGB);
                attrList.AppendChild(attr);
            }

            // 設定ファイル生成
            using (FileStream stream = new FileStream(configFileName, FileMode.Create))
            {
                xml.Save(stream);
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

        /// <summary>
        /// 画像対して、ラベル付け作業を行う。
        /// </summary>
        /// <param name="area">ラベル付けを行う範囲</param>
        /// <param name="label">ラベル付けを行う値(属性の出力画素値)</param>
        /// <param name="disp">表示用に設定する色()</param>
        /// <returns></returns>
        public override Boolean LabelingImage(Int32Rect area, byte label, Color disp)
        {
            // ラベル画像に対するラベリング
            if(!Utility.ToolUtility.ChangePixelData(ref _LabelImageData, area, label))
                return false;

            // 表示画像に対するラベリング
            //if (!Utility.Utility.ChangePixelData(_LabelImageData, area, disp))
            //    return false;

            return true;            
        }

        #endregion
    }
}
