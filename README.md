# [C#中如何串行执行所有线程](https://www.jianshu.com/p/c78a6a799059)

>为建立中文知识库加块砖
　　　　　　　　——中科大胡不归

### 0、前言
第一次在技术群提出这个问题，大佬们一脸懵逼，既然你要串行执行，为什么不单线程。其实就是由于很多场景需要封装的业务逻辑，并依赖线程，比如顺序写日志文件。
当然这是我现阶段粗浅水平所能想到的实现方法，希望以后能接触到更广大的世界，能嘲笑今天的自己。

### 1、实现原理
#### AutoResetEvent
依赖AutoResetEvent的信号机制实现“串行”。
对象方法：
- Set表示设置为有信号状态，这时调用WaitOne的线程将继续执行；
- Reset表示设置为无信号状态，这时调用WaitOne的线程将阻塞；
- WaitOne表示在无信号状态时阻塞当前线程，也就是说WaitOne只有在无信号状态下才会阻塞线程。

因为AutoResetEvent调用Set后会在第一个调用WaitOne后，自动将信号置为无信号状态，导致其他调用WaitOne的线程继续阻塞。

#### Queue
使用Queue的“先进先出”特性实现“顺序”。
最早使用ThreadPool，但是不能控制线程顺序被唤醒。也可能是未得其法，希望有大神看到能指正用法。

### 2、代码
正文只贴关键代码：
```c#
public partial class MainWindow
    {
        private readonly Queue<CookieTask> _threadQueue = new Queue<CookieTask>();
        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(true);
        private int _count = 10;

        public MainWindow()
        {
            InitializeComponent();
            
            var task  = new Thread(SerialService);
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

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            for (var i = 0; i < 100; i++)
            {
                var tt1 = new Thread(DemoTask);
                _threadQueue.Enqueue(new CookieTask(tt1, _count--));
                if (_count <= 0)
                {
                    _count = 10;
                }
            }
        }

        private void DemoTask(object seconds)
        {
            _autoResetEvent.WaitOne();

            int times = int.Parse(seconds.ToString());
            while (times-- > 0) 
            {
                Debug.Write(int.Parse(seconds.ToString()) + " ");
                Thread.Sleep(10);
            }
            Debug.WriteLine("");

            _autoResetEvent.Set();
        }
    }
```
[完整工程](https://github.com/zhongwcool/SerialExecutionDemo)
我们在初始化AutoResetEvent对象时，将初始为true即表示有信号状态，所以第一个提交的DemoTask中的WaitOne能拿到信号。

AutoResetEvent在第一个调用WaitOne后，自动将信号置为无信号状态导致其他调用WaitOne阻塞。

### 参考文章：
1. [WPF下多线程的使用方法](https://www.cnblogs.com/yangyancheng/archive/2011/04/05/2006227.html)
2. [C#线程控制ManualResetEvent和AutoResetEvent](https://blog.csdn.net/chtnj/article/details/8114399)
