using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<int, EventWaitHandle> _dictionary =
            new ConcurrentDictionary<int, EventWaitHandle>();

        public MainWindow()
        {
            InitializeComponent();

            //这种定时器不是工作在UI线程
            var timer = new Timer(2000);
            timer.Elapsed += MyElapsedEventHandler; //注册中断事件
            timer.Start(); //启动定时器
        }

        private void MyElapsedEventHandler(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => { info.Text = "欢迎你光临WPF的世界,Dispatcher 异步方法！！" + DateTime.Now; });
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            for (var i = 0; i < 10; i++)
            {
                _dictionary.TryAdd(i, new AutoResetEvent(false));
                ThreadPool.QueueUserWorkItem(DemoTask, i);
            }

            _dictionary[0].Set();
        }

        private void DemoTask(object key)
        {
            var index = (int) key;
            var id = DateTime.Now.ToString("MM-dd HH:mm:ss fff");
            Debug.WriteLine("START @" + id);
            _dictionary[index].WaitOne();

            int times;
            if (index % 2 == 0)
                times = 10;
            else
                times = 2;

            while (times-- > 0)
            {
                Debug.Write(times + " ");
                Thread.Sleep(100);
            }

            Debug.WriteLine("");

            Debug.WriteLine("END @" + id);
            if (_dictionary.ContainsKey(index + 1)) _dictionary[index + 1].Set();
        }
    }
}