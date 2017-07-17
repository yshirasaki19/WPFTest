using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Media.Imaging;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using ImageLabelingTool_pre.Models;
using System.Collections.ObjectModel;

namespace ImageLabelingTool_pre.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        /* コマンド、プロパティの定義にはそれぞれ 
         * 
         *  lvcom   : ViewModelCommand
         *  lvcomn  : ViewModelCommand(CanExecute無)
         *  llcom   : ListenerCommand(パラメータ有のコマンド)
         *  llcomn  : ListenerCommand(パラメータ有のコマンド・CanExecute無)
         *  lprop   : 変更通知プロパティ(.NET4.5ではlpropn)
         *  
         * を使用してください。
         * 
         * Modelが十分にリッチであるならコマンドにこだわる必要はありません。
         * View側のコードビハインドを使用しないMVVMパターンの実装を行う場合でも、ViewModelにメソッドを定義し、
         * LivetCallMethodActionなどから直接メソッドを呼び出してください。
         * 
         * ViewModelのコマンドを呼び出せるLivetのすべてのビヘイビア・トリガー・アクションは
         * 同様に直接ViewModelのメソッドを呼び出し可能です。
         */

        /* ViewModelからViewを操作したい場合は、View側のコードビハインド無で処理を行いたい場合は
         * Messengerプロパティからメッセージ(各種InteractionMessage)を発信する事を検討してください。
         */

        /* Modelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedEventListenerや
         * CollectionChangedEventListenerを使うと便利です。各種ListenerはViewModelに定義されている
         * CompositeDisposableプロパティ(LivetCompositeDisposable型)に格納しておく事でイベント解放を容易に行えます。
         * 
         * ReactiveExtensionsなどを併用する場合は、ReactiveExtensionsのCompositeDisposableを
         * ViewModelのCompositeDisposableプロパティに格納しておくのを推奨します。
         * 
         * LivetのWindowテンプレートではViewのウィンドウが閉じる際にDataContextDisposeActionが動作するようになっており、
         * ViewModelのDisposeが呼ばれCompositeDisposableプロパティに格納されたすべてのIDisposable型のインスタンスが解放されます。
         * 
         * ViewModelを使いまわしたい時などは、ViewからDataContextDisposeActionを取り除くか、発動のタイミングをずらす事で対応可能です。
         */

        /* UIDispatcherを操作する場合は、DispatcherHelperのメソッドを操作してください。
         * UIDispatcher自体はApp.xaml.csでインスタンスを確保してあります。
         * 
         * LivetのViewModelではプロパティ変更通知(RaisePropertyChanged)やDispatcherCollectionを使ったコレクション変更通知は
         * 自動的にUIDispatcher上での通知に変換されます。変更通知に際してUIDispatcherを操作する必要はありません。
         */

        public void Initialize()
        {
            ITiffImage = new TiffImage();
            LabelAttributes = new ObservableCollection<LabelAttribute>();
            LabelAttributes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(LabelAttributes_CollectionChanged);
            LabelAttributes.Add(new LabelAttribute("属性A(下地)", 0, "#FFFFFF"));
            LabelAttributes.Add(new LabelAttribute("属性B(未設定)", 255, "#000000"));
            RaisePropertyChanged("LabelAttributes");
        }


        #region ITiffImage変更通知プロパティ
        private ITiffImage _ITiffImage;

        public ITiffImage ITiffImage
        {
            get
            { return _ITiffImage; }
            set
            { 
                if (_ITiffImage == value)
                    return;
                _ITiffImage = value;
                RaisePropertyChanged("ITiffImage");
            }
        }
        #endregion

        #region FileName変更通知プロパティ
        private string _FileName;

        public string FileName
        {
            get { return ITiffImage != null ? System.IO.Path.GetFileName(ITiffImage.FileFullPath) : null; }
            set
            { 
                if (_FileName == value)
                    return;
                _FileName = value;
                RaisePropertyChanged("FileName");
            }
        }
        #endregion

        #region ExpansionRate変更通知プロパティ
        private int _ExpansionRate = 100;

        public int ExpansionRate
        {
            get
            { return _ExpansionRate; }
            set
            {
                if (_ExpansionRate == value)
                    return;
                _ExpansionRate = value;
                if (ITiffImage.BitmapImage != null)
                {
                    _ImageHeiht = _ExpansionRate * ITiffImage.BitmapImage.PixelHeight / 100;
                    _ImageWidth = _ExpansionRate * ITiffImage.BitmapImage.PixelWidth / 100;
                    RaisePropertyChanged("ImageHeiht");
                    RaisePropertyChanged("ImageWidth");
                }
            }
        }
        #endregion

        #region BitmapImage変更通知プロパティ
        private BitmapImage _BitmapImage;

        public BitmapImage BitmapImage
        {
            get { return ITiffImage != null ? ITiffImage.BitmapImage : null; }
            set
            { 
                if (_BitmapImage == value)
                    return;
                _BitmapImage = value;
                RaisePropertyChanged("BitmapImage");
            }
        }
        #endregion

        #region ImageHeiht変更通知プロパティ
        private int _ImageHeiht;

        public int ImageHeiht
        {
            get
            { return _ImageHeiht; }
            set
            { 
                if (_ImageHeiht == value)
                    return;
                _ImageHeiht = value;
                RaisePropertyChanged("ImageHeight");
            }
        }
        #endregion

        #region ImageWidth変更通知プロパティ
        private int _ImageWidth;

        public int ImageWidth
        {
            get
            { return _ImageWidth; }
            set
            { 
                if (_ImageWidth == value)
                    return;
                _ImageWidth = value;
                RaisePropertyChanged("ImageWidth");
            }
        }
        #endregion

        #region LabelAttribute変更通知プロパティ
        private LabelAttribute _LabelAttribute;

        public LabelAttribute LabelAttribute
        {
            get
            { return _LabelAttribute; }
            set
            { 
                if (_LabelAttribute == value)
                    return;
                _LabelAttribute = value;
                RaisePropertyChanged("LabelAttribute");
            }
        }
        #endregion

        #region LabelAttributes変更通知プロパティ
        private ObservableCollection<LabelAttribute> _LabelAttributes;
        public ObservableCollection<LabelAttribute> LabelAttributes
        {
            get
            { return _LabelAttributes; }
            set
            {
                if (_LabelAttributes == value)
                    return;
                _LabelAttributes = value;
                RaisePropertyChanged("LabelAttributes");
            }
        }

        void LabelAttributes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("LabelAttributes");
        }
        #endregion

        /// <summary>
        /// メニュー 新規作成
        /// </summary>
        #region NewTiffImageCommand
        private ViewModelCommand _NewTiffImageCommand;

        public ViewModelCommand NewTiffImageCommand
        {
            get
            {
                if (_NewTiffImageCommand == null)
                {
                    _NewTiffImageCommand = new ViewModelCommand(NewTiffImage);
                }
                return _NewTiffImageCommand;
            }
        }

        public void NewTiffImage()
        {
            // 保存ダイアログを表示する。
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = "新規作成";
            dialog.Filter = "Tiff Bitmap画像|*.tiff;*.tif;*.bmp";
            if (dialog.ShowDialog() == true)
            {
                ITiffImage.FileFullPath = dialog.FileName;
                _ImageHeiht = ITiffImage.BitmapImage.PixelHeight;
                _ImageWidth = ITiffImage.BitmapImage.PixelWidth;
                RefreshAllItems();
            } else
            {
                Messenger.Raise(new InformationMessage("キャンセルしました", "ファイル保存", System.Windows.MessageBoxImage.Information, "Info"));
            }
        }
        #endregion
    
        /// <summary>
        /// すべてのアイテムに更新通知を発行する。
        /// </summary>
        public void RefreshAllItems()
        {
            RaisePropertyChanged("BitmapImage");
            RaisePropertyChanged("FileName");
            RaisePropertyChanged("ImageHeiht");
            RaisePropertyChanged("ImageWidth");
        }
    }
}
