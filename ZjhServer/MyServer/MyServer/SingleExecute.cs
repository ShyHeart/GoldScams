using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyServer
{
    /// <summary>
    /// 一个要执行的方法（委托）
    /// </summary>
    public delegate void ExecuteDelegate();
    public class SingleExecute
    {
        private static object ob = new object();
        private static SingleExecute instance = null;
        public static SingleExecute Instance   //实例
        {

            get
            {
                //加锁 防止instance 被多个线程同时访问  new出多个对象
                //当一个线程访问到instance时，会将代码锁上
                lock (ob)
                {

                if (instance == null)
                {
                    instance = new SingleExecute();

                }
                return instance;
                }
            }
        }

        private object objLock = new object();

        /// <summary>
        /// 互斥锁
        /// </summary>
        private Mutex _mutex;

        public SingleExecute()
        {
            _mutex = new Mutex();
        }

        /// <summary>
        /// 单线程执行逻辑
        /// </summary>
        /// <param name="executeDelegate"></param>
        public void Execute(ExecuteDelegate executeDelegate)
        {
            lock (objLock)
            {
                _mutex.WaitOne();
                executeDelegate();
                _mutex.ReleaseMutex(); //释放
            }
        }
    }
}
