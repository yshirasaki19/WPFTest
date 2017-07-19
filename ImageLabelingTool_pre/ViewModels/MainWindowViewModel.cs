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
using System.IO;

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

        // モデル
        private TiffImage TargetImage;

        public void Initialize()
        {
            TargetImage = new TiffImage();
            LabelAttributes = new ObservableCollection<LabelAttribute>();
            LabelAttributes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(LabelAttributes_CollectionChanged);
            LabelAttributes.Add(new LabelAttribute("属性A(下地)", 0, "#FFFFFF"));
            LabelAttributes.Add(new LabelAttribute("属性B(未設定)", 255, "#000000"));
            RaisePropertyChanged("LabelAttributes");
            StatusMessage = "起動完了";
        }


        #region StatusMessage変更通知プロパティ
        private string _StatusMessage;

        public string StatusMessage
        {
            get
            { return _StatusMessage; }
            set
            { 
                if (_StatusMessage == value)
                    return;
                _StatusMessage = value;
                RaisePropertyChanged("StatusMessage");
            }
        }
        #endregion

        #region FileName変更通知プロパティ
        private string _FileName;

        public string FileName
        {
            get { return TargetImage != null ? Path.GetFileName(TargetImage.BaseFileFullPath) : null; }
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
                if (TargetImage.DisplayImage != null)
                {
                    _ImageHeiht = _ExpansionRate * TargetImage.DisplayImage.PixelHeight / 100;
                    _ImageWidth = _ExpansionRate * TargetImage.DisplayImage.PixelWidth / 100;
                    RefreshAllItems();
                }
            }
        }
        #endregion

        #region BitmapImage変更通知プロパティ
        private BitmapImage _BitmapImage;

        public BitmapImage BitmapImage
        {
            get { return TargetImage != null ? (BitmapImage)TargetImage.DisplayImage : null; }
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

        #region OpenNewImageFileCommand(メニュー：ファイル → 新規作成)
        private ViewModelCommand _OpenNewImageFileCommand;

        public ViewModelCommand OpenNewImageFileCommand
        {
            get
            {
                if (_OpenNewImageFileCommand == null)
                {
                    _OpenNewImageFileCommand = new ViewModelCommand(OpenNewImageFile);
                }
                return _OpenNewImageFileCommand;
            }
        }

        public void OpenNewImageFile()
        {
            if (TargetImage.OpenNewImageFile())
            {
                _ImageHeiht = _ExpansionRate * TargetImage.DisplayImage.PixelHeight / 100;
                _ImageWidth = _ExpansionRate * TargetImage.DisplayImage.PixelWidth / 100;
                RefreshAllItems();
            } else
            {
                Messenger.Raise(new InformationMessage("キャンセルしました", "新規作成", System.Windows.MessageBoxImage.Information, "Info"));
            }
        }
        #endregion

        #region OpenAttributeFileCommand(メニュー：ファイル → 開く)
        private ViewModelCommand _OpenAttributeFileCommand;

        public ViewModelCommand OpenAttributeFileCommand
        {
            get
            {
                if (_OpenAttributeFileCommand == null)
                {
                    _OpenAttributeFileCommand = new ViewModelCommand(OpenAttributeFile);
                }
                return _OpenAttributeFileCommand;
            }
        }

        public void OpenAttributeFile()
        {
            if (TargetImage.OpenAttributeFile())
            {
                RefreshAllItems();
            }
            else
            {
                Messenger.Raise(new InformationMessage("キャンセルしました", "開く", System.Windows.MessageBoxImage.Information, "Info"));
            }
        }
        #endregion

        #region SaveAttributeFileCommand(メニュー：ファイル → 一時保存)
        private ViewModelCommand _SaveAttributeFileCommand;

        public ViewModelCommand SaveAttributeFileCommand
        {
            get
            {
                if (_SaveAttributeFileCommand == null)
                {
                    _SaveAttributeFileCommand = new ViewModelCommand(SaveAttributeFile);
                }
                return _SaveAttributeFileCommand;
            }
        }

        public void SaveAttributeFile()
        {
            if (TargetImage.SaveAttributeFile())
            {
                RefreshAllItems();
            }
            else
            {
                Messenger.Raise(new InformationMessage("キャンセルしました", "一時保存", System.Windows.MessageBoxImage.Information, "Info"));
            }
        }
        #endregion

        #region ClopImageFileCommand(メニュー：ファイル → 切り出し)
        private ViewModelCommand _ClopImageFileCommand;

        public ViewModelCommand ClopImageFileCommand
        {
            get
            {
                if (_ClopImageFileCommand == null)
                {
                    _ClopImageFileCommand = new ViewModelCommand(ClopImageFile);
                }
                return _ClopImageFileCommand;
            }
        }

        public void ClopImageFile()
        {
            if (TargetImage.ClopImageFile())
            {
                RefreshAllItems();
            }
            else
            {
                Messenger.Raise(new InformationMessage("キャンセルしました", "一時保存", System.Windows.MessageBoxImage.Information, "Info"));
            }
        }
        #endregion

        #region ExitApplicationCommand(メニュー：ファイル → 終了)
        private ViewModelCommand _ExitApplicationCommand;

        public ViewModelCommand ExitApplicationCommand
        {
            get
            {
                if (_ExitApplicationCommand == null)
                {
                    _ExitApplicationCommand = new ViewModelCommand(ExitApplication, CanExitApplication);
                }
                return _ExitApplicationCommand;
            }
        }

        public bool CanExitApplication()
        {
            return true;
        }

        public void ExitApplication()
        {
            App.Current.MainWindow.Close();
        }
        #endregion

        #region OpenClopSizeWindowCommandCommand(メニュー：設定 → 切取サイズ)
        private ViewModelCommand _OpenClopSizeWindowCommand;

        public ViewModelCommand OpenClopSizeWindowCommand
        {
            get
            {
                if (_OpenClopSizeWindowCommand == null)
                {
                    _OpenClopSizeWindowCommand = new ViewModelCommand(OpenClopSizeWindow);
                }
                return _OpenClopSizeWindowCommand;
            }
        }

        public void OpenClopSizeWindow()
        {
            using (var vm = new ClopSizeWindowViewModel(this.TargetImage))
            {
                Messenger.Raise(new TransitionMessage(vm, "OpenClopSizeWindow"));
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
            RaisePropertyChanged("StatusMessage");
        }
    }
}
