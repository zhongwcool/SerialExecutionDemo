#[C#中如何串行执行所有线程](https://www.jianshu.com/p/c78a6a799059)

>为建立中文知识库加块砖
　　　　　　　　——中科大胡不归

### 0、前言
第一次在技术群提出这个问题，大佬们一脸懵逼，既然你要串行执行，为什么不单线程。其实就是由于很多场景需要封装的业务逻辑，并依赖线程，比如顺序写日志文件。
当然这是我现阶段粗浅水平所能想到的实现方法，希望以后能接触到更广大的世界，能嘲笑今天的自己。

###1、实现原理
主要依赖AutoResetEvent的信号机制实现线程控制。
主要的对象方法：Set、Reset、WaitOne，其中：
- Set表示设置为有信号状态，这时调用WaitOne的线程将继续执行；
- Reset表示设置为无信号状态，这时调用WaitOne的线程将阻塞；
- WaitOne表示在无信号状态时阻塞当前线程，也就是说WaitOne只有在无信号状态下才会阻塞线程。

因为AutoResetEvent调用Set后会在第一个调用WaitOne后，自动将信号置为无信号状态，导致其他调用WaitOne的线程继续阻塞。

###2、代码
正文只贴关键代码：
```c#
namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        //此处是设置初始状态false表示无信号状态，true表示有信号状态
        private AutoResetEvent autoResetEvent = new AutoResetEvent(true);

        public MainWindow()
        {
            InitializeComponent();
            
            ThreadPool.SetMaxThreads(100, 10);
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(DemoTask), 10);
            ThreadPool.QueueUserWorkItem(new WaitCallback(DemoTask), 2);
            ThreadPool.QueueUserWorkItem(new WaitCallback(DemoTask), 3);
        }

        private void DemoTask(object seconds)
        {
            String id = DateTime.Now.ToString("MM-dd HH:mm:ss.fff");
            Console.WriteLine("START " + id);
            autoResetEvent.WaitOne();

            int times = int.Parse(seconds.ToString());
            while (times-- > 0) 
            {
                Console.WriteLine(times);
                Thread.Sleep(1000);
            }

            Console.WriteLine("END " + id);
            autoResetEvent.Set();
        }
    }
}
```

我们在初始化AutoResetEvent对象时，将初始为true即表示有信号状态，所以第一个提交的DemoTask中的WaitOne能拿到信号。
AutoResetEvent在第一个调用WaitOne后，自动将信号置为无信号状态导致其他调用WaitOne阻塞。
所以依次提交的任务依次获得信号，其他则阻塞等待，实现了顺序执行。

###参考文章：
1. [WPF下多线程的使用方法](https://www.cnblogs.com/yangyancheng/archive/2011/04/05/2006227.html)
2. [C#线程控制ManualResetEvent和AutoResetEvent](https://blog.csdn.net/chtnj/article/details/8114399)

