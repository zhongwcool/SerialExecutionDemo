# [C#中如何串行执行所有线程](https://www.jianshu.com/p/c78a6a799059)

>为建立中文知识库加块砖
　　　　　　　　——中科大胡不归

### 0、前言
第一次在技术群提出这个问题，大佬们一脸懵逼，既然你要串行执行，为什么不单线程。其实就是由于很多场景需要封装的业务逻辑，并依赖线程，比如顺序写日志文件。
当然这是我现阶段粗浅水平所能想到的实现方法，希望以后能接触到更广大的世界，能嘲笑今天的自己。

### 1、实现原理
依赖AutoResetEvent的信号机制实现线程控制。
对象方法：
- Set表示设置为有信号状态，这时调用WaitOne的线程将继续执行；
- Reset表示设置为无信号状态，这时调用WaitOne的线程将阻塞；
- WaitOne表示在无信号状态时阻塞当前线程，也就是说WaitOne只有在无信号状态下才会阻塞线程。
因为AutoResetEvent调用Set后会在第一个调用WaitOne后，自动将信号置为无信号状态，导致其他调用WaitOne的线程继续阻塞。

### 2、代码
正文只贴关键代码：
```c#
public partial class MainWindow
    {
        private readonly ConcurrentDictionary<int, EventWaitHandle> _dictionary = new ConcurrentDictionary<int, EventWaitHandle>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                _dictionary.TryAdd(i, new AutoResetEvent(false));
                ThreadPool.QueueUserWorkItem(DemoTask, i);
            }

            _dictionary[0].Set();
        }

        private void DemoTask(object key)
        {
            int index = (int)key;
            String id = DateTime.Now.ToString("MM-dd HH:mm:ss fff");
            Debug.WriteLine("START @" + id);
            _dictionary[index].WaitOne();

            int times;
            if (index % 2 == 0)
            {
                times = 10;
            }
            else
            {
                times = 2;
            }

            while (times-- > 0) 
            {
                Debug.Write(times + " ");
                Thread.Sleep(100);
            }
            Debug.WriteLine("");

            Debug.WriteLine("END @" + id);
            if (_dictionary.ContainsKey(index + 1))
            {
                _dictionary[index + 1].Set();
            }
        }
    }
```
[完整工程](https://github.com/zhongwcool/SerialExecutionDemo)


### 参考文章：
1. [WPF下多线程的使用方法](https://www.cnblogs.com/yangyancheng/archive/2011/04/05/2006227.html)
2. [C#线程控制ManualResetEvent和AutoResetEvent](https://blog.csdn.net/chtnj/article/details/8114399)

