using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;

namespace WpfApp2
{// 测试数据模型（示例）
    public class MyDataItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _id;

        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        private string _name;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private double _price;

        public double Price
        {
            get
            {
                return _price;
            }
            set
            {
                _price = value;
                OnPropertyChanged(nameof(Price));
            }
        }

        private DateTime _createTime;

        public DateTime CreateTime
        {
            get
            {
                return _createTime;
            }
            set
            {
                _createTime = value; OnPropertyChanged(nameof(CreateTime));
            }
        }
    }

    public class DataGridContextMenuViewModel : INotifyPropertyChanged
    {
        // INotifyPropertyChanged实现
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 动态菜单项集合（绑定到ContextMenu.ItemsSource）
        public ObservableCollection<MenuItemModel> ContextMenuItems { get; } = new ObservableCollection<MenuItemModel>();

        // 当前选中的单元格信息（类型、值、所在行/列等）
        private CellInfo _currentCellInfo;

        public CellInfo CurrentCellInfo
        {
            get => _currentCellInfo;
            set
            {
                _currentCellInfo = value;
                OnPropertyChanged();
                // 当选中单元格变化时，更新菜单
                UpdateContextMenuByCellType();
            }
        }

        public ObservableCollection<MyDataItem> DataItems { get; set; }

        // 构造函数：初始化演示数据
        public DataGridContextMenuViewModel()
        {
            DataItems = new ObservableCollection<MyDataItem>
            {
                new MyDataItem
                {
                    Id = 1,
                    Name = "laptop computer",
                    Price = 5999.99,
                    CreateTime = new DateTime(2024, 1, 15)
                },
                new MyDataItem
                {
                    Id = 2,
                    Name = "wireless mouse",
                    Price = 129,
                    CreateTime = new DateTime(2024, 3, 2)
                },
                new MyDataItem
                {
                    Id = 3,
                    Name = "mechanical keyboard",
                    Price = 349.5,
                    CreateTime = new DateTime(2024, 2, 20)
                },
                new MyDataItem
                {
                    Id = 4,
                    Name = "27-inch monitor",
                    Price = 1899,
                    CreateTime = new DateTime(2024, 4, 5)
                },
                new MyDataItem
                {
                    Id = 5,
                    Name = "bluetooth headset",
                    Price = 799.9,
                    CreateTime = new DateTime(2024, 1, 30)
                }
            };
        }

        // 根据单元格类型更新菜单
        private void UpdateContextMenuByCellType()
        {
            ContextMenuItems.Clear();
            if (CurrentCellInfo?.CellType == null)
            {
                return;
            }

            // 根据类型添加对应菜单
            if (CurrentCellInfo.CellType == typeof(int) || CurrentCellInfo.CellType == typeof(double))
            {
                // 数字类型：添加“加1”“减1”按钮
                ContextMenuItems.Add(new MenuItemModel
                {
                    Header = "加1",
                    Command = new RelayCommand(AddOne)
                });
                ContextMenuItems.Add(new MenuItemModel
                {
                    Header = "减1",
                    Command = new RelayCommand(SubtractOne)
                });
            }
            else if (CurrentCellInfo.CellType == typeof(string))
            {
                // 字符串类型：添加“转大写”“转小写”按钮
                ContextMenuItems.Add(new MenuItemModel
                {
                    Header = "转大写",
                    Command = new RelayCommand(ToUpper)
                });
                ContextMenuItems.Add(new MenuItemModel
                {
                    Header = "转小写",
                    Command = new RelayCommand(ToLower)
                });
            }
            else if (CurrentCellInfo.CellType == typeof(DateTime))
            {
                // 日期类型：添加“加1天”按钮
                ContextMenuItems.Add(new MenuItemModel
                {
                    Header = "加1天",
                    Command = new RelayCommand(AddOneDay)
                });
            }
        }

        // 菜单命令实现（示例）
        private void AddOne()
        {
            if (CurrentCellInfo?.DataItem == null || string.IsNullOrEmpty(CurrentCellInfo.PropertyName))
                return;

            // 获取原始数据对象的属性
            var property = CurrentCellInfo.DataItem.GetType().GetProperty(CurrentCellInfo.PropertyName);
            if (property == null) return;

            // 根据属性类型修改值（直接修改原始对象的属性，触发PropertyChanged）
            if (property.PropertyType == typeof(int))
            {
                var value = (int)property.GetValue(CurrentCellInfo.DataItem);
                property.SetValue(CurrentCellInfo.DataItem, value + 1);
            }
            else if (property.PropertyType == typeof(double))
            {
                var value = (double)property.GetValue(CurrentCellInfo.DataItem);
                property.SetValue(CurrentCellInfo.DataItem, value + 1);
            }
        }

        private void SubtractOne()
        {
            if (CurrentCellInfo?.DataItem == null || string.IsNullOrEmpty(CurrentCellInfo.PropertyName))
                return;

            var property = CurrentCellInfo.DataItem.GetType().GetProperty(CurrentCellInfo.PropertyName);
            if (property == null) return;

            if (property.PropertyType == typeof(int))
            {
                var value = (int)property.GetValue(CurrentCellInfo.DataItem);
                property.SetValue(CurrentCellInfo.DataItem, value - 1);
            }
            else if (property.PropertyType == typeof(double))
            {
                var value = (double)property.GetValue(CurrentCellInfo.DataItem);
                property.SetValue(CurrentCellInfo.DataItem, value - 1);
            }
        }

        private void ToUpper()
        {
            // 检查必要参数（原始数据对象、属性名）
            if (CurrentCellInfo?.DataItem == null || string.IsNullOrEmpty(CurrentCellInfo.PropertyName))
                return;

            // 获取原始数据对象的属性（如Name）
            var property = CurrentCellInfo.DataItem.GetType().GetProperty(CurrentCellInfo.PropertyName);
            if (property == null || property.PropertyType != typeof(string))
                return; // 确保是string类型

            // 读取当前值 → 转大写 → 写回原始属性（触发PropertyChanged）
            var currentValue = (string)property.GetValue(CurrentCellInfo.DataItem);
            if (currentValue != null)
            {
                property.SetValue(CurrentCellInfo.DataItem, currentValue.ToUpper());
            }
        }

        private void ToLower()
        {
            if (CurrentCellInfo?.DataItem == null || string.IsNullOrEmpty(CurrentCellInfo.PropertyName))
                return;

            var property = CurrentCellInfo.DataItem.GetType().GetProperty(CurrentCellInfo.PropertyName);
            if (property == null || property.PropertyType != typeof(string))
                return;

            var currentValue = (string)property.GetValue(CurrentCellInfo.DataItem);
            if (currentValue != null)
            {
                property.SetValue(CurrentCellInfo.DataItem, currentValue.ToLower());
            }
        }

        private void AddOneDay()
        {
            if (CurrentCellInfo?.DataItem == null || string.IsNullOrEmpty(CurrentCellInfo.PropertyName))
                return;

            // 获取原始数据对象的属性（如CreateTime）
            var property = CurrentCellInfo.DataItem.GetType().GetProperty(CurrentCellInfo.PropertyName);
            if (property == null || property.PropertyType != typeof(DateTime))
                return; // 确保是DateTime类型

            // 读取当前日期 → 加1天 → 写回原始属性（触发PropertyChanged）
            var currentValue = (DateTime)property.GetValue(CurrentCellInfo.DataItem);
            property.SetValue(CurrentCellInfo.DataItem, currentValue.AddDays(1));
        }
    }

    // 菜单项模型（绑定到ContextMenu的Item）
    public class MenuItemModel
    {
        public string Header { get; set; } // 菜单文本
        public ICommand Command { get; set; } // 菜单命令
    }

    // 单元格信息模型（存储类型、值等）
    public class CellInfo
    {
        public Type CellType { get; set; } // 单元格数据类型
        public object Value { get; set; } // 单元格当前值
        public DataGridCell Cell { get; set; } // 单元格对象（可选，用于更复杂操作）
        public object DataItem { get; set; } // 新增：单元格所在行的原始数据对象（MyDataItem实例）
        public string PropertyName { get; set; } // 新增：绑定的属性名（如"Price"）
    }

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

        private void DataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var cell = FindVisualParent<DataGridCell>((DependencyObject)e.OriginalSource);
                if (cell == null) return;

                MyDataGrid.SelectedCells.Clear();
                MyDataGrid.SelectedCells.Add(new DataGridCellInfo(cell));
                cell.Focus();

                var dataContext = cell.DataContext; // 这就是当前行的MyDataItem实例
                var column = cell.Column as DataGridBoundColumn;
                if (column?.Binding == null)
                {
                    return;
                }

                var bindingPath = (column.Binding as Binding)?.Path.Path; // 属性名（如"Price"）
                if (string.IsNullOrEmpty(bindingPath))
                {
                    return;
                }

                var property = dataContext.GetType().GetProperty(bindingPath);
                if (property == null)
                {
                    return;
                }

                (DataContext as DataGridContextMenuViewModel).CurrentCellInfo = new CellInfo
                {
                    CellType = property.PropertyType,
                    Value = property.GetValue(dataContext),
                    Cell = cell,
                    DataItem = dataContext, // 保存原始数据对象
                    PropertyName = bindingPath // 保存属性名
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 辅助方法：从视觉树中查找父级控件（如DataGridCell）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        private T FindVisualParent<T>(DependencyObject obj) where T : DependencyObject
        {
            while (obj != null)
            {
                if (obj is T t) return t;
                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
        }
    }
}