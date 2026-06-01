using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;

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

    /// <summary>
    /// BindWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BindWindow : Window
    {
        public BindWindow()
        {
            InitializeComponent();
        }
    }
}