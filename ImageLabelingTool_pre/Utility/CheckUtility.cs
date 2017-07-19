using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLabelingTool_pre.Utility
{
    public sealed class CheckUtility
    {
        private static CheckUtility _instance = new CheckUtility();

        private CheckUtility()
        {
        }

        public static CheckUtility GetInstance()
        {
            return _instance;
        }

        internal static bool CheckClopCutSize(string _ClopSize, int p)
        {
            // 数値であること
            if (int.TryParse(_ClopSize, out p))
            {
                // 指定範囲内であること
                if (32 <= p || p <= 128)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
