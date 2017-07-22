using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ImageLabelingTool_pre.Models
{
    [Serializable]
    public class LabelAttributes : NotificationObject, ISerializable
    {
        private readonly string FILE_NAME_SERIALIZE = "preAttribute.data";
        private ObservableCollection<LabelAttribute> _Attributes;
        public ObservableCollection<LabelAttribute> Attributes
        {
            get { return _Attributes; }
            set { _Attributes = value; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LabelAttributes()
        {
            _Attributes = new ObservableCollection<LabelAttribute>();
            _Attributes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(LabelAttributes_CollectionChanged);
        }

        protected LabelAttributes(SerializationInfo info, StreamingContext context)
        {
            _Attributes = (ObservableCollection<LabelAttribute>)info.GetValue("Attributes", typeof(ObservableCollection<LabelAttribute>));
            _Attributes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(LabelAttributes_CollectionChanged);
        } 
        /// <summary>
        /// コレクション変更時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void LabelAttributes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("LabelAttributes");
        }

        /// <summary>
        /// 
        /// </summary>
        public void CreateSerializeData()
        {
            // 次回実行時用に属性情報をシリアライズ
            using (FileStream stream = new FileStream(FILE_NAME_SERIALIZE, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ReadSerializeData()
        {
            try
            {
                using (FileStream stream = new FileStream(FILE_NAME_SERIALIZE, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    LabelAttributes tmp;
                    tmp = (LabelAttributes)formatter.Deserialize(stream);
                    foreach (LabelAttribute attr in tmp.Attributes)
                        _Attributes.Add(new LabelAttribute(attr.Name, attr.Pixel, attr.RGB));
                    if (_Attributes.Count == 0 )
                        throw new Exception("No AttributeData");
                }
            }
            catch(Exception e)
            {
                Debug.Print(e.StackTrace);

                // ファイルが無い場合は初期値設定
                _Attributes.Add(new LabelAttribute("属性A(下地)", 0, "#FFFFFF"));
                _Attributes.Add(new LabelAttribute("属性B(未設定)", 255, "#000000"));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            try
            {
                info.AddValue("Attributes", _Attributes);
            }
            catch (Exception e)
            {
                Debug.Print(e.StackTrace);
            }
        }
    }
}
