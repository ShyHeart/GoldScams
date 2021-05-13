using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Constant
{
    /// <summary>
    /// 手牌类型
    /// </summary>
    public enum CardType
    {
        None,
        /// <summary>
        /// 单张
        /// </summary>
        Min,
        /// <summary>
        /// 对子
        /// </summary>
        Duizi,
        /// <summary>
        /// 顺子
        /// </summary>
        Shunzi,
        /// <summary>
        /// 金花
        /// </summary>
        Jinhua,
        /// <summary>
        /// 顺金
        /// </summary>
        Shunjin,
        /// <summary>
        /// 豹子
        /// </summary>
        Baozi,
        /// <summary>
        /// 235
        /// </summary>
        Max,
    }
}
