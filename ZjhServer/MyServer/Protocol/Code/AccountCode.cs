using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Code
{
    /// <summary>
    /// 账户的子操作码
    /// </summary>
    public class AccountCode
    {
        public const int Register_CREQ = 0;  //客户端请求注册
        public const int Register_SRES = 1;  //服务器响应
        public const int Login_CREQ = 2;     //登录客户端
        public const int Login_SRES = 3;     //服务器响应
        public const int GetUserInfo_CREQ = 4;
        public const int GetUserInfo_SRES = 5;
        public const int GetRankList_CREQ = 6;
        public const int GetRankList_SRES = 7;

        public const int UpdateCoinCount_CREQ = 8;  //更新金币的操作码
        public const int UpdateCoinCount_SRES = 9;

    }
}
