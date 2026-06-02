using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp2
{
    public static class BindingExtensions
    {
        /// <summary>
        /// WPF 专用：根据属性路径字符串（如 "A.B.C"）自动绑定控件到对象嵌套属性
        /// </summary>
        public static void SetBindingByPath(this Control control, object dataObj, string propertyPath)
        {
            if (dataObj == null || string.IsNullOrEmpty(propertyPath))
                return;

            // 取值并显示到控件
            object value = GetNestedPropertyValue(dataObj, propertyPath);
            control.DataContext = dataObj;
            if (control is TextBox tb) tb.Text = value?.ToString() ?? "";
            else if (control is ComboBox cb) cb.Text = value?.ToString() ?? "";
            else if (control is Label lbl) lbl.Content = value?.ToString() ?? "";

            // 可继续加其他控件

            // 离开控件时自动写回（双向绑定）
            control.LostFocus += (s, e) =>
            {
                string text = string.Empty;
                if (control is TextBox t) text = t.Text;
                else if (control is ComboBox c) text = c.Text;
                else if (control is Label l) text = l.Content?.ToString() ?? "";

                SetNestedPropertyValue(dataObj, propertyPath, text);
            };
        }

        /// <summary>
        /// 反射：根据路径获取嵌套属性
        /// </summary>
        public static object GetNestedPropertyValue(object obj, string path)
        {
            foreach (string prop in path.Split('.'))
            {
                if (obj == null) break;
                PropertyInfo pi = obj.GetType().GetProperty(prop);
                obj = pi.GetValue(obj);
            }
            return obj;
        }

        /// <summary>
        /// 反射：根据路径设置嵌套属性
        /// </summary>
        public static void SetNestedPropertyValue(object obj, string path, string text)
        {
            var properties = path.Split('.');
            object target = obj;

            // 走到最后一级父对象
            for (int i = 0; i < properties.Length - 1; i++)
            {
                PropertyInfo pi = target.GetType().GetProperty(properties[i]);
                target = pi.GetValue(target);
                if (target == null) return;
            }

            // 设置最后一级属性
            string lastProp = properties[properties.Length - 1];
            PropertyInfo propInfo = target.GetType().GetProperty(lastProp);
            Type targetType = propInfo.PropertyType;

            // 自动类型转换
            object safeValue = Convert.ChangeType(text, Nullable.GetUnderlyingType(targetType) ?? targetType);
            propInfo.SetValue(target, safeValue);
        }
    }

    public class User
    {
        public Address Info { get; set; }
        public int Age { get; set; }
    }

    public class Address
    {
        public string City { get; set; }
        public double Height { get; set; }
    }

    /// <summary>
    /// BindWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BindWindow : Window
    {
        public BindWindow()
        {
            InitializeComponent();

            BindControls();
        }

        private void BindControls()
        {
            UIElement ui = new UIElement();

            TextBox_Test.Text = "Test";

            ui = TextBox_Test;

            ListBox lbox = new ListBox();
            ListView lv = new ListView();
            ComboBox cb = new ComboBox();
            DataGrid dg = new DataGrid();
            TreeView treeView = new TreeView();
            MenuItem menuItem = new MenuItem();
            ContextMenu contextMenu = new ContextMenu();
            TabControl tabControl = new TabControl();
            ListBoxItem listBoxItem = new ListBoxItem();
            ListViewItem listViewItem = new ListViewItem();
            ComboBoxItem comboBoxItem = new ComboBoxItem();
            TreeViewItem treeViewItem = new TreeViewItem();
            TabItem tabItem = new TabItem();

            TextBox textBox = new TextBox();
            PasswordBox passwordBox = new PasswordBox();
            RichTextBox richTextBox = new RichTextBox();
            CheckBox checkBox = new CheckBox();
            RadioButton radioButton = new RadioButton();
            Button button = new Button();
            ComboBox comboBox = new ComboBox();
            ListBox listBox = new ListBox();
            ListView listView = new ListView();
            Slider slider = new Slider();
            ProgressBar progressBar = new ProgressBar();
            DatePicker datePicker = new DatePicker();
            Calendar calendar = new Calendar();

            Canvas canvas = new Canvas();
            DockPanel dockPanel = new DockPanel();
            Grid grid = new Grid();
            UniformGrid uniformGrid = new UniformGrid();
            StackPanel stackPanel = new StackPanel();
            WrapPanel wrapPanel = new WrapPanel();
            VirtualizingStackPanel virtualizingStackPanel = new VirtualizingStackPanel();
            TabPanel tabPanel = new TabPanel();
            ToolBarPanel toolBarPanel = new ToolBarPanel();
            ToolBarOverflowPanel toolBarOverflowPanel = new ToolBarOverflowPanel();
            WrapPanel wrapPanel1 = new WrapPanel();

            Window window = new Window();
            UserControl userControl = new UserControl();
            Border border = new Border();
            Viewbox viewbox = new Viewbox();
            Label label = new Label();
            TextBlock textBlock = new TextBlock();
            AccessText accessText = new AccessText();
            GroupBox groupBox = new GroupBox();
            Expander expander = new Expander();
            HeaderedContentControl headeredContentControl = new HeaderedContentControl();
            ToolTip toolTip = new ToolTip();

            DataGrid dataGrid = new DataGrid();
            ItemsControl itemsControl = new ItemsControl();
            ContentControl contentControl = new ContentControl();
            HeaderedItemsControl headeredItemsControl = new HeaderedItemsControl();

            Menu menu = new Menu();
            MenuItem menuItem1 = new MenuItem();
            ContextMenu contextMenu1 = new ContextMenu();
            ToolBar toolBar = new ToolBar();
            ToolBarTray toolBarTray = new ToolBarTray();
            StatusBar statusBar = new StatusBar();
            StatusBarItem statusBarItem1 = new StatusBarItem();
            Separator separator = new Separator();

            Image image = new Image();
            MediaElement mediaElement = new MediaElement();
            Viewport3D viewport3D = new Viewport3D();

            FlowDocumentReader flowDocumentReader = new FlowDocumentReader();
            FlowDocumentPageViewer flowDocumentPageViewer = new FlowDocumentPageViewer();
            FlowDocumentScrollViewer flowDocumentScrollViewer = new FlowDocumentScrollViewer();
            RichTextBox richTextBox1 = new RichTextBox();
            BlockUIContainer blockUIContainer = new BlockUIContainer();
            InlineUIContainer inlineUIContainer = new InlineUIContainer();

            var user = new User
            {
                Age = 25,
                Info = new Address { City = "北京", Height = 178.5 }
            };

            TextBox txtCity = new TextBox();
            TextBox txtAge = new TextBox();
            ComboBox cbHeight = new ComboBox();
            cbHeight.SetBinding(ComboBox.ItemsSourceProperty, new Binding("Info.Height"));

            // Create OneWay Binding
            Binding binding = new Binding
            {
                Path = new PropertyPath("Name"),   // Property name on DataContext
                Mode = BindingMode.OneWay          // OneWay = Source → Target
            };

            txtCity.SetBinding(TextBox.TextProperty, binding);
            // 一句话绑定深层属性
            txtCity.SetBindingByPath(user, "Info.City");
            txtAge.SetBindingByPath(user, "Age");
            cbHeight.SetBindingByPath(user, "Info.Height");
        }
    }
}