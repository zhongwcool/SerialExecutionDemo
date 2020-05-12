using System;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using System.Windows;
using Timer = System.Timers.Timer;

namespace SerialExecutionDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly Timer _timer;
        private AutoResetEvent autoResetEvent = new AutoResetEvent(true);

        public MainWindow()
        {
            InitializeComponent();
            
            //这种定时器不是工作在UI线程
            _timer = new Timer(2000);//实例化Timer类，设置间隔时间为20毫秒；  
            _timer.Elapsed += MyElapsedEventHandler; //注册中断事件
            _timer.Start();//启动定时器
            
            ThreadPool.SetMaxThreads(100, 10);
        }

        private void MyElapsedEventHandler(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.info.Text = "欢迎你光临WPF的世界,Dispatcher 异步方法！！"+ DateTime.Now;
            });
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            /*
            Thread tt1 = new Thread(DemoTask);
            tt1.Start(10);
            Thread tt2 = new Thread(DemoTask);
            tt2.Start(2);
            Thread tt3 = new Thread(DemoTask);
            tt3.Start(3);
            */
            ThreadPool.QueueUserWorkItem(new WaitCallback(DemoTask), 10);
            ThreadPool.QueueUserWorkItem(new WaitCallback(DemoTask), 2);
            ThreadPool.QueueUserWorkItem(new WaitCallback(DemoTask), 3);
        }

        private void DemoTask(object seconds)
        {
            String id = DateTime.Now.ToString("MM-dd HH:mm:ss.fff");
            Debug.WriteLine("START " + id);
            autoResetEvent.WaitOne();

            int times = int.Parse(seconds.ToString());
            while (times-- > 0) 
            {
                Debug.Write(times + " ");
                Thread.Sleep(1000);
            }
            Debug.WriteLine("");

            Debug.WriteLine("END " + id);
            autoResetEvent.Set();
        }
    }
}