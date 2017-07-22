using System;
using System.Windows.Media.Imaging;

using Livet;
using Livet.Commands;
using Livet.Messaging;

using ImageLabelingTool_pre.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Runtime.Serialization.Formatters.Binary;

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
        private LabelAttributes TargetAttributes;

        // ラベル、クロップ処理制御
        private Boolean _CanClopAndLabeling;

        // MessangerKey
        private readonly string MESSENGER_KEY_OPEN_INFO = "OpenInfoDialog";
        private readonly string MESSENGER_KEY_OPEN_CONFIRM = "OpenConfirmDialog";
        private readonly string MESSENGER_KEY_OPEN_MODAL = "OpenClopSizeWindow";

        public void Initialize()
        {
            this.TargetImage = new TiffImage();
            TargetAttributes = new LabelAttributes();
            this.LabelAttributes = TargetAttributes.Attributes;
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
                    RaiseAllPropertyChanged();
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

        #region SelectedLabelAttribute変更通知プロパティ
        private LabelAttribute _SelectedLabelAttribute;

        public LabelAttribute SelectedLabelAttribute
        {
            get
            { return _SelectedLabelAttribute; }
            set
            { 
                if (_SelectedLabelAttribute == value)
                    return;
                _SelectedLabelAttribute = value;
                RaisePropertyChanged("SelectedLabelAttribute");
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
            string message = "";

            // 処理中である場合は、破棄確認のメッセージを表示する。
            if (TargetImage !=null && TargetImage.DisplayImage != null)
            {
                message = "現在編集中の情報が破棄されますが、新規にファイルを取込ますか？";
                ConfirmationMessage confirmMessage = new ConfirmationMessage(message, "新規作成", MessageBoxImage.Question, MessageBoxButton.YesNo, MESSENGER_KEY_OPEN_CONFIRM);
                Messenger.Raise(confirmMessage);
                if (!(confirmMessage.Response.HasValue && confirmMessage.Response.Value))
                {
                    return;
                }
            }

            // 画像ファイルを開き、初期値を画面項目に値を反映する。
            if (TargetImage.OpenNewImageFile(ref message))
            {
                // 属性情報は前回起動時の情報を取得
                TargetAttributes.ReadSerializeData();

                // 画面情報を更新
                _ImageHeiht = _ExpansionRate * TargetImage.DisplayImage.PixelHeight / 100;
                _ImageWidth = _ExpansionRate * TargetImage.DisplayImage.PixelWidth / 100;
                _rectH = TargetImage.DisplayImage.PixelHeight;      // 暫定(矩形選択実装後は不要)
                _rectW = TargetImage.DisplayImage.PixelWidth;       // 暫定(矩形選択実装後は不要)
                _CanClopAndLabeling = true;
                StatusMessage = "画像表示完了";
                RaiseAllPropertyChanged();
            }
            else
                Messenger.Raise(new InformationMessage(message, "エラー", MessageBoxImage.Exclamation, MESSENGER_KEY_OPEN_INFO));
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
            string message = "";

            // 処理中である場合は、破棄確認のメッセージを表示する。
            if (TargetImage != null && TargetImage.DisplayImage != null)
            {
                message = "現在編集中の情報が破棄されますが、ファイルを開きますか？";
                ConfirmationMessage confirmMessage = new ConfirmationMessage(message, "開く", MessageBoxImage.Question, MessageBoxButton.YesNo, MESSENGER_KEY_OPEN_CONFIRM);
                Messenger.Raise(confirmMessage);
                if (!(confirmMessage.Response.HasValue && confirmMessage.Response.Value))
                {
                    return;
                }
            }

            // 設定ファイルを開く。成功するとラベル、クロップが可能となる。
            if (TargetImage.OpenAttributeFile(ref message, ref TargetAttributes))
            {
                // 画面情報を更新
                _ImageHeiht = _ExpansionRate * TargetImage.DisplayImage.PixelHeight / 100;
                _ImageWidth = _ExpansionRate * TargetImage.DisplayImage.PixelWidth / 100;
                _rectH = TargetImage.DisplayImage.PixelHeight;      // 暫定(矩形選択実装後は不要)
                _rectW = TargetImage.DisplayImage.PixelWidth;       // 暫定(矩形選択実装後は不要)
                _CanClopAndLabeling = true;
                StatusMessage = "画像表示完了";
                RaiseAllPropertyChanged();
            }
            else
                Messenger.Raise(new InformationMessage(message, "エラー", MessageBoxImage.Exclamation, MESSENGER_KEY_OPEN_INFO));

            RaiseAllPropertyChanged();
        }
        #endregion

        #region SaveFileCommand(メニュー：ファイル → 保存)
        private ViewModelCommand _SaveFileCommand;
        public ViewModelCommand SaveFileCommand
        {
            get
            {
                if (_SaveFileCommand == null)
                {
                    _SaveFileCommand = new ViewModelCommand(SaveFile, CanSaveFile);
                }
                return _SaveFileCommand;
            }
        }

        public bool CanSaveFile()
        {
            if (TargetImage == null || TargetImage.LabelImageData == null)
                return false;
            else
                return true;
        }
        public void SaveFile()
        {
            if (TargetImage.SaveImageFile(TargetAttributes))
            {
                StatusMessage = "保存完了";
                RaiseAllPropertyChanged();
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
                    _ClopImageFileCommand = new ViewModelCommand(ClopImageFile, CanClopImageFile);
                }
                return _ClopImageFileCommand;
            }
        }

        public bool CanClopImageFile()
        {
            if (TargetImage == null || TargetImage.LabelImageData == null)
                return false;
            else if (!_CanClopAndLabeling)
                return false;
            else
                return true;
        }

        public void ClopImageFile()
        {
            if (TargetImage.ClopImageFile())
            {
                
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
                    _ExitApplicationCommand = new ViewModelCommand(ExitApplication);
                }
                return _ExitApplicationCommand;
            }
        }

        public void ExitApplication()
        {
            // 次回実行用に属性の値を保持する。
            TargetAttributes.CreateSerializeData();
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
                Messenger.Raise(new TransitionMessage(vm, MESSENGER_KEY_OPEN_MODAL));
            }
        }
        #endregion

        #region LabelingCommand(画面選択)
        private ViewModelCommand _LabelingCommand;
        public ViewModelCommand LabelingCommand
        {
            get
            {
                if (_LabelingCommand == null)
                {
                    _LabelingCommand = new ViewModelCommand(Labeling, CanLabeling);
                }
                return _LabelingCommand;
            }
        }

        public bool CanLabeling()
        {
            if (TargetImage == null || TargetImage.LabelImageData == null)
                return false;
            else if (!_CanClopAndLabeling)
                return false;
            else
                return true;
        }

        public async void Labeling()
        {
            Boolean result = true;
            String stackTrace = "";

            // ラベル処理中は再実行不可、ステータスバーに通知
            _StatusMessage = "ラベリング処理中…";
            _CanClopAndLabeling = false;
            RaiseAllPropertyChanged();

            // ラベリングはUIとは別のスレッドで実施する。
            Func<Task> act = async () =>
            {
                try
                {
                    Int32Rect area = new Int32Rect(rectX, rectY, rectW, rectH);
                    byte label = _SelectedLabelAttribute.Pixel;
                    Color disp =  (Color)(ColorConverter.ConvertFromString(_SelectedLabelAttribute.RGB));
                    result = TargetImage.LabelingImage(area, label, disp);
                }
                catch (Exception e)
                {
                    stackTrace = e.StackTrace; 
                }
                await Task.Delay(1000);
            };
            await act();
            
            if (!result)
                Messenger.Raise(new InformationMessage("例外発生:\n" + stackTrace, "ラベリング処理", System.Windows.MessageBoxImage.Warning, MESSENGER_KEY_OPEN_INFO));

            // ラベル処理完了
            _StatusMessage = "ラベリング完了";
            _CanClopAndLabeling = true;
            RaiseAllPropertyChanged();
        }
        #endregion

        /// <summary>
        /// すべてのアイテムに更新通知を発行する。
        /// </summary>
        public void RaiseAllPropertyChanged()
        {
            RaisePropertyChanged("BitmapImage");
            RaisePropertyChanged("FileName");
            RaisePropertyChanged("ImageHeiht");
            RaisePropertyChanged("ImageWidth");
            RaisePropertyChanged("LabelAttribute");
            RaisePropertyChanged("LabelAttributes");
            RaisePropertyChanged("StatusMessage");
            SaveFileCommand.RaiseCanExecuteChanged();
            ClopImageFileCommand.RaiseCanExecuteChanged();
            LabelingCommand.RaiseCanExecuteChanged();

            RaisePropertyChanged("rectX");
            RaisePropertyChanged("rectY");
            RaisePropertyChanged("rectW");
            RaisePropertyChanged("rectH");
        }

        #region 矩形選択ができるまでの暫定コード
        
        #region rectX変更通知プロパティ
        private int _rectX;

        public int rectX
        {
            get
            { return _rectX; }
            set
            { 
                if (_rectX == value)
                    return;
                _rectX = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region rectY変更通知プロパティ
        private int _rectY;

        public int rectY
        {
            get
            { return _rectY; }
            set
            { 
                if (_rectY == value)
                    return;
                _rectY = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region rectW変更通知プロパティ
        private int _rectW;

        public int rectW
        {
            get
            { return _rectW; }
            set
            { 
                if (_rectW == value)
                    return;
                _rectW = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region rectH変更通知プロパティ
        private int _rectH;

        public int rectH
        {
            get
            { return _rectH; }
            set
            { 
                if (_rectH == value)
                    return;
                _rectH = value;
                RaisePropertyChanged();
            }
        }
        #endregion
        #endregion
    }
}
