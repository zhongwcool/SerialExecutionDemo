using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using System.Windows;
using Timer = System.Timers.Timer;

namespace SerialExecutionDemo
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(true);
        private readonly Queue<CookieTask> _threadQueue = new Queue<CookieTask>();
        private int _count = 10;

        public MainWindow()
        {
            InitializeComponent();

            //这种定时器不是工作在UI线程
            var timer = new Timer(2000);
            timer.Elapsed += MyElapsedEventHandler; //注册中断事件
            timer.Start(); //启动定时器

            var task = new Thread(SerialService);
            task.Start();
        }

        private void SerialService()
        {
            while (true)
            {
                if (_threadQueue.Count > 0)
                {
                    var tt = _threadQueue.Dequeue();
                    tt.Worker.Start(tt.Data);
                }

                Thread.Sleep(500);
            }
        }

        private void MyElapsedEventHandler(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => { info.Text = "欢迎你光临WPF的世界,Dispatcher 异步方法！！" + DateTime.Now; });
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            for (var i = 0; i < 100; i++)
            {
                var tt1 = new Thread(DemoTask);
                _threadQueue.Enqueue(new CookieTask(tt1, _count--));
                if (_count <= 0) _count = 10;
            }
        }

        private void DemoTask(object seconds)
        {
            _autoResetEvent.WaitOne();

            var times = int.Parse(seconds.ToString());
            while (times-- > 0)
            {
                Debug.Write(int.Parse(seconds.ToString()) + " ");
                Thread.Sleep(10);
            }

            Debug.WriteLine("");

            _autoResetEvent.Set();
        }
    }
}