using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Dto
{
    /// <summary>
    /// 账户传输模型
    /// </summary>
    [Serializable]
    public class AccountDto
    {
        public string userName;
        public string passWord;

        public AccountDto(string userName, string passWord)
        {
            this.userName = userName;
            this.passWord = passWord;

        }

        public void Change(string userName, string passWord)
        {
            this.userName = userName;
            this.passWord = passWord;
        }
    }
}
