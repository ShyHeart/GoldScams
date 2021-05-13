using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MyServer.TimerTool
{
    /// <summary>
    /// 计时器任务管理类
    /// </summary>
   public class TimerManager
    {
        /// <summary>
        /// 单例模式
        /// </summary>
        

        private static object ObjLock=new object();
        private static TimerManager instance = null;

        public static TimerManager Instance
        {
            get
            {
                lock (ObjLock)
                {
                    if (instance == null)
                    {
                        instance = new TimerManager();
                    }

                    return instance;

                }
            }
        }
        /// <summary>
        /// 计时器重要的类
        /// </summary>
        private Timer timer;
        /// <summary>
        /// 计时器任务数据模型Id与模型映射字典
        /// </summary>
        private ConcurrentDictionary<int,TimerModel> idModelDic=new ConcurrentDictionary<int, TimerModel>();

        private ThreadSafeInt id = new ThreadSafeInt(-1);

        public TimerManager()
        {
            //1000毫秒，就是1秒
            timer=new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var item in idModelDic.Values)
            {
                //100纳秒 千万分一秒
                if (DateTime.Now.Ticks>=item.time)
                {
                    item.Run();
                }
            }
        }
        /// <summary>
        /// 添加计时任务
        /// </summary>
        /// <param name="deayTime"></param>
        /// <param name="td"></param>
        public void AddTimerEvent(float deayTime, TimerDelegate td)
        {
            if (deayTime <= 0) return;
            TimerModel model = new TimerModel(id.Add_Get(), DateTime.Now.Ticks + (long) (deayTime * 10000000.0), td);
            idModelDic.TryAdd(model.id,model);
        }
        /// <summary>
        /// 清空字典
        /// </summary>
        public void Clear()
        {
            idModelDic.Clear();
        }
    }
}
