using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp2
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        // 线程安全队列：接收线程 -> 解析线程的数据管道
        private readonly ConcurrentQueue<byte[]> _dataQueue = new ConcurrentQueue<byte[]>();

        // 接收线程的控制（取消令牌+运行状态）
        private CancellationTokenSource _receiveCts;

        private bool _isReceiving;

        // 解析线程的控制（取消令牌+运行状态）
        private CancellationTokenSource _parseCts;

        private bool _isParsing;

        public MainWindow()
        {
            InitializeComponent();
        }

        // 启动接收线程和解析线程（先启动解析线程，确保能接收数据）
        private void StartAllTasks()
        {
            if (!_isParsing) StartParseTask();
            if (!_isReceiving) StartReceiveTask();
        }

        // 线程1：接收网络数据
        private void StartReceiveTask()
        {
            _receiveCts = new CancellationTokenSource();
            var token = _receiveCts.Token;
            _isReceiving = true;

            // 更新UI：接收线程启动
            UpdateReceiveStatus("接收线程启动，开始监听网络...");

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // 模拟网络接收（实际项目中替换为Socket/SerialPort读取）
                        byte[] rawData = await ReceiveFromNetwork();

                        if (rawData != null && rawData.Length > 0)
                        {
                            // 数据入队（线程安全）
                            _dataQueue.Enqueue(rawData);
                            UpdateReceiveStatus($"接收数据：{BitConverter.ToString(rawData)}（已入队）");
                        }

                        // 模拟接收间隔（根据实际需求调整，如100ms一次）
                        await Task.Delay(100, token);
                    }
                    catch (Exception ex)
                    {
                        // 接收线程异常：通知UI，停止当前线程
                        UpdateReceiveStatus($"接收线程异常：{ex.Message}");
                        MessageBox.Show($"接收错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        _isReceiving = false;
                        break;
                    }
                }
            }, token);
        }

        // 线程2：解析报文并处理
        private void StartParseTask()
        {
            _parseCts = new CancellationTokenSource();
            var token = _parseCts.Token;
            _isParsing = true;

            // 更新UI：解析线程启动
            UpdateParseStatus("解析线程启动，等待数据...");

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // 从队列取数据（线程安全）
                        if (_dataQueue.TryDequeue(out byte[] rawData))
                        {
                            // 解析报文（实际项目中替换为Modbus协议解析逻辑）
                            string result = ParseModbusData(rawData);

                            UpdateParseStatus($"解析成功：{result}");

                            // 执行后续处理（如更新设备状态、触发业务逻辑）
                            ProcessParsedData(result);
                        }
                        else
                        {
                            // 队列空时短暂等待，避免CPU空转
                            await Task.Delay(50, token);
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateParseStatus($"解析线程异常：{ex.Message}");
                        MessageBox.Show($"解析错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        _isParsing = false;
                        break;
                    }
                }
            }, token);
        }

        // 模拟网络接收（实际实现：用Socket/SerialPort读取Modbus数据）
        private Task<byte[]> ReceiveFromNetwork()
        {
            // 模拟30%概率接收失败（如网络中断）
            if (new Random().Next(10) < 3)
            {
                throw new InvalidOperationException("网络超时，未收到数据");
            }

            // 模拟接收到的Modbus报文（示例：功能码0x03，寄存器地址0x0001，数据0x1234）
            return Task.FromResult(new byte[] { 0x01, 0x03, 0x00, 0x01, 0x00, 0x01, 0x12, 0x34 });
        }

        // 解析Modbus报文（实际实现：根据Modbus协议解析功能码、数据区等）
        private string ParseModbusData(byte[] rawData)
        {
            // 模拟30%概率解析失败（如CRC校验错误）
            if (new Random().Next(10) < 3)
            {
                throw new InvalidOperationException("报文格式错误，CRC校验失败");
            }

            // 简单解析示例：提取数据区
            return $"设备地址：{rawData[0]}, 功能码：{rawData[1]}, 数据：{BitConverter.ToInt16(rawData, 6)}";
        }

        // 处理解析后的数据（如更新UI、触发业务逻辑）
        private void ProcessParsedData(string result)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                DeviceStatusTextBlock.Text = $"最新状态：{result}";
            });
        }

        // 更新接收线程状态（UI线程）
        private void UpdateReceiveStatus(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ReceiveStatusTextBlock.Text = $"[{DateTime.Now:HH:mm:ss}] 接收：{message}";
            });
        }

        // 更新解析线程状态（UI线程）
        private void UpdateParseStatus(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ParseStatusTextBlock.Text = $"[{DateTime.Now:HH:mm:ss}] 解析：{message}";
            });
        }

        // 重启按钮：重启所有停止的线程
        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            StartAllTasks();
        }

        // 窗口关闭时：终止所有线程，释放资源
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // 取消接收线程（不再接收新数据）
            _receiveCts?.Cancel();
            // 等待解析线程处理完剩余数据后取消（可选）
            Task.Delay(1000).ContinueWith(_ => _parseCts?.Cancel());

            base.OnClosing(e);
        }
    }
}