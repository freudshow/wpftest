using System;
using System.Collections.Generic;
using System.Data;
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

namespace WpfApp2
{
    // 扩展方法需放在静态类中
    public static class DataColumnCollectionExtensions
    {
        /// <summary>
        /// 模拟DataColumnCollection的InsertAt方法
        /// </summary>
        /// <param name="columns">列集合</param>
        /// <param name="column">要插入的列</param>
        /// <param name="index">插入的目标索引</param>
        public static void InsertAt(this DataColumnCollection columns, DataColumn column, int index)
        {
            if (columns == null) throw new ArgumentNullException(nameof(columns));
            if (column == null) throw new ArgumentNullException(nameof(column));
            if (index < 0 || index > columns.Count) throw new ArgumentOutOfRangeException(nameof(index));

            // 先添加列，再调整索引
            columns.Add(column);
            column.SetOrdinal(index);
        }
    }

    /// <summary>
    /// DataCell.xaml 的交互逻辑
    /// </summary>
    public partial class DataCell : Window
    {
        #region 全局变量

        private DataTable _dataTable; // 数据源（DataTable更灵活支持动态列）
        private bool _isDraggingFillHandle; // 是否正在拖动填充柄
        private Point _dragStartPoint; // 拖动起始点
        private DataGridCell _startCell; // 序列填充的起始单元格

        #endregion 全局变量

        public DataCell()
        {
            InitializeComponent();
        }

        #region 1. 复制功能（Ctrl+C）

        private void CopySelectedCells()
        {
            if (excelDataGrid.SelectedCells.Count == 0) return;

            // 整理选中单元格数据（按Excel格式：Tab分隔列，换行分隔行）
            var cellValues = new Dictionary<int, Dictionary<int, string>>();
            int minRow = int.MaxValue, maxRow = int.MinValue;
            int minCol = int.MaxValue, maxCol = int.MinValue;

            // 遍历选中单元格，记录行/列索引和值
            foreach (var cell in excelDataGrid.SelectedCells)
            {
                int rowIndex = cell.Item as DataRowView != null ? (cell.Item as DataRowView).Row.Table.Rows.IndexOf((cell.Item as DataRowView).Row) : -1;
                int colIndex = cell.Column.DisplayIndex;

                if (rowIndex < 0) continue;

                // 更新行列边界
                minRow = Math.Min(minRow, rowIndex);
                maxRow = Math.Max(maxRow, rowIndex);
                minCol = Math.Min(minCol, colIndex);
                maxCol = Math.Max(maxCol, colIndex);

                // 存储单元格值
                if (!cellValues.ContainsKey(rowIndex))
                    cellValues[rowIndex] = new Dictionary<int, string>();
                cellValues[rowIndex][colIndex] = cell.Item != null ? cell.Column.GetCellContent(cell.Item)?.ToString() ?? "" : "";
            }

            // 拼接复制文本
            var copyText = new System.Text.StringBuilder();
            for (int row = minRow; row <= maxRow; row++)
            {
                for (int col = minCol; col <= maxCol; col++)
                {
                    if (cellValues.ContainsKey(row) && cellValues[row].ContainsKey(col))
                        copyText.Append(cellValues[row][col]);
                    if (col < maxCol) copyText.Append("\t"); // Tab分隔列
                }
                if (row < maxRow) copyText.Append(Environment.NewLine); // 换行分隔行
            }

            // 写入剪贴板
            Clipboard.SetText(copyText.ToString());
        }

        #endregion 1. 复制功能（Ctrl+C）

        #region 2. 粘贴功能（Ctrl+V）

        private void PasteToSelectedCells()
        {
            if (excelDataGrid.SelectedCells.Count == 0 || string.IsNullOrEmpty(Clipboard.GetText())) return;

            // 获取选中的第一个单元格作为粘贴起始位置
            var firstCell = excelDataGrid.SelectedCells[0];
            int startRow = firstCell.Item as DataRowView != null ? (firstCell.Item as DataRowView).Row.Table.Rows.IndexOf((firstCell.Item as DataRowView).Row) : 0;
            int startCol = firstCell.Column.DisplayIndex;

            // 解析剪贴板数据（按Excel格式拆分）
            string[] rows = Clipboard.GetText().Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            // 逐行粘贴
            for (int i = 0; i < rows.Length; i++)
            {
                int targetRow = startRow + i;
                if (targetRow >= _dataTable.Rows.Count)
                {
                    // 超出现有行则新增行
                    DataRow newRow = _dataTable.NewRow();
                    _dataTable.Rows.Add(newRow);
                }

                // 拆分列
                string[] cols = rows[i].Split('\t');
                for (int j = 0; j < cols.Length; j++)
                {
                    int targetCol = startCol + j;
                    if (targetCol >= _dataTable.Columns.Count)
                    {
                        // 超出现有列则新增列
                        _dataTable.Columns.Add($"新增列{_dataTable.Columns.Count + 1}", typeof(object));
                    }

                    // 赋值（避免索引越界）
                    if (targetRow < _dataTable.Rows.Count && targetCol < _dataTable.Columns.Count)
                    {
                        _dataTable.Rows[targetRow][targetCol] = cols[j];
                    }
                }
            }
        }

        #endregion 2. 粘贴功能（Ctrl+V）

        #region 3. 序列填充（鼠标拖动填充柄自增）

        // 步骤1：鼠标按下时检测是否点击填充柄
        private void ExcelDataGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            // 获取鼠标点击的单元格
            _startCell = GetCellUnderMouse(e) as DataGridCell;
            if (_startCell == null) return;

            // 检测是否点击填充柄（单元格右下角8x8区域）
            var fillHandleRect = new Rect(
                _startCell.ActualWidth - 8,
                _startCell.ActualHeight - 8,
                8, 8);
            Point mousePosInCell = e.GetPosition(_startCell);

            _isDraggingFillHandle = fillHandleRect.Contains(mousePosInCell);
            if (_isDraggingFillHandle)
            {
                _dragStartPoint = e.GetPosition(excelDataGrid);
                e.Handled = true; // 阻止默认行为
            }
        }

        // 步骤2：鼠标拖动时绘制选中区域并准备填充
        private void ExcelDataGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingFillHandle || _startCell == null) return;

            // 获取拖动终点单元格
            var endCell = GetCellUnderMouse(e) as DataGridCell;
            if (endCell == null) return;

            // 计算填充的行列范围
            int startRow = GetRowIndex(_startCell);
            int startCol = GetColumnIndex(_startCell);
            int endRow = GetRowIndex(endCell);
            int endCol = GetColumnIndex(endCell);

            // 选中填充范围（视觉反馈）
            SelectCellsInRange(startRow, startCol, endRow, endCol);
        }

        // 步骤3：鼠标松开时执行序列填充
        private void ExcelDataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDraggingFillHandle || _startCell == null)
            {
                _isDraggingFillHandle = false;
                _startCell = null;
                return;
            }

            // 获取拖动终点单元格
            var endCell = GetCellUnderMouse(e) as DataGridCell;
            if (endCell == null)
            {
                _isDraggingFillHandle = false;
                _startCell = null;
                return;
            }

            // 执行序列填充
            FillSequence(_startCell, endCell);

            // 重置状态
            _isDraggingFillHandle = false;
            _startCell = null;
            e.Handled = true;
        }

        // 核心：序列填充逻辑（支持数字、日期、字母+数字自增）
        private void FillSequence(DataGridCell startCell, DataGridCell endCell)
        {
            // 获取起始单元格值和行列索引
            object startValue = GetCellValue(startCell);
            if (startValue == DBNull.Value || startValue == null) return;

            int startRow = GetRowIndex(startCell);
            int startCol = GetColumnIndex(startCell);
            int endRow = GetRowIndex(endCell);
            int endCol = GetColumnIndex(endCell);

            // 判断填充方向（行/列）
            bool isRowFill = Math.Abs(endRow - startRow) > Math.Abs(endCol - startCol);
            int step = isRowFill ? (endRow > startRow ? 1 : -1) : (endCol > startCol ? 1 : -1);
            int count = isRowFill ? Math.Abs(endRow - startRow) : Math.Abs(endCol - startCol);

            // 解析起始值类型并生成序列
            for (int i = 1; i <= count; i++)
            {
                object nextValue = GetNextSequenceValue(startValue, i);
                int targetRow = isRowFill ? startRow + (i * step) : startRow;
                int targetCol = isRowFill ? startCol : startCol + (i * step);

                // 赋值（避免越界）
                if (targetRow >= 0 && targetRow < _dataTable.Rows.Count &&
                    targetCol >= 0 && targetCol < _dataTable.Columns.Count)
                {
                    _dataTable.Rows[targetRow][targetCol] = nextValue;
                }
            }
        }

        // 生成下一个序列值（支持数字、日期、字母+数字）
        private object GetNextSequenceValue(object startValue, int step)
        {
            string startStr = startValue.ToString()?.Trim() ?? "";

            // 1. 数字类型（整数/小数）
            if (double.TryParse(startStr, out double num))
            {
                return num + step;
            }

            // 2. 日期类型
            if (DateTime.TryParse(startStr, out DateTime date))
            {
                return date.AddDays(step);
            }

            // 3. 字母+数字（如A1、B2、X100）
            if (IsAlphaNumeric(startStr))
            {
                // 拆分字母和数字部分
                string alphaPart = "", numPart = "";
                foreach (char c in startStr)
                {
                    if (char.IsLetter(c)) alphaPart += c;
                    else if (char.IsDigit(c)) numPart += c;
                }
                if (int.TryParse(numPart, out int seqNum))
                {
                    return $"{alphaPart}{seqNum + step}";
                }
            }

            // 4. 纯文本：直接复制
            return startValue;
        }

        // 辅助：判断是否是字母+数字组合
        private bool IsAlphaNumeric(string str)
        {
            bool hasAlpha = false, hasNum = false;
            foreach (char c in str)
            {
                if (char.IsLetter(c)) hasAlpha = true;
                else if (char.IsDigit(c)) hasNum = true;
                if (hasAlpha && hasNum) return true;
            }
            return false;
        }

        #endregion 3. 序列填充（鼠标拖动填充柄自增）

        #region 4. 插入行/列功能

        // 插入行（上方）
        private void InsertRowAbove_Click(object sender, RoutedEventArgs e)
        {
            InsertRow(true);
        }

        // 插入行（下方）
        private void InsertRowBelow_Click(object sender, RoutedEventArgs e)
        {
            InsertRow(false);
        }

        // 插入列（左侧）
        private void InsertColumnLeft_Click(object sender, RoutedEventArgs e)
        {
            InsertColumn(true);
        }

        // 插入列（右侧）
        private void InsertColumnRight_Click(object sender, RoutedEventArgs e)
        {
            InsertColumn(false);
        }

        // 插入行核心逻辑
        private void InsertRow(bool insertAbove)
        {
            if (excelDataGrid.SelectedItem is DataRowView selectedRow)
            {
                int rowIndex = _dataTable.Rows.IndexOf(selectedRow.Row);
                DataRow newRow = _dataTable.NewRow();
                // 插入到指定位置
                _dataTable.Rows.InsertAt(newRow, insertAbove ? rowIndex : rowIndex + 1);
            }
            else
            {
                // 无选中行则插入到最后
                _dataTable.Rows.Add(_dataTable.NewRow());
            }
        }

        // 插入列核心逻辑
        private void InsertColumn(bool insertLeft)
        {
            if (excelDataGrid.CurrentColumn != null)
            {
                int colIndex = excelDataGrid.CurrentColumn.DisplayIndex;
                string newColName = $"新增列{_dataTable.Columns.Count + 1}";
                // 插入到指定位置
                _dataTable.Columns.InsertAt(new DataColumn(newColName, typeof(object)),
                    insertLeft ? colIndex : colIndex + 1);
            }
            else
            {
                // 无选中列则添加到最后
                _dataTable.Columns.Add($"新增列{_dataTable.Columns.Count + 1}", typeof(object));
            }
        }

        #endregion 4. 插入行/列功能

        #region 辅助方法

        // 获取鼠标下的单元格
        private UIElement GetCellUnderMouse(MouseEventArgs e)
        {
            Point pos = e.GetPosition(excelDataGrid);
            return excelDataGrid.InputHitTest(pos) as UIElement;
        }

        // 获取单元格的行索引
        private int GetRowIndex(DataGridCell cell)
        {
            if (cell.DataContext is DataRowView rowView)
            {
                return _dataTable.Rows.IndexOf(rowView.Row);
            }
            return -1;
        }

        // 获取单元格的列索引
        private int GetColumnIndex(DataGridCell cell)
        {
            return cell.Column.DisplayIndex;
        }

        // 获取单元格的值
        private object GetCellValue(DataGridCell cell)
        {
            if (cell.DataContext is DataRowView rowView)
            {
                return rowView.Row[cell.Column.DisplayIndex];
            }
            return null;
        }

        // 选中指定范围的单元格
        private void SelectCellsInRange(int startRow, int startCol, int endRow, int endCol)
        {
            excelDataGrid.SelectedCells.Clear();
            // 交换行列确保范围正确
            int minRow = Math.Min(startRow, endRow);
            int maxRow = Math.Max(startRow, endRow);
            int minCol = Math.Min(startCol, endCol);
            int maxCol = Math.Max(startCol, endCol);

            // 遍历范围并选中单元格
            for (int row = minRow; row <= maxRow; row++)
            {
                for (int col = minCol; col <= maxCol; col++)
                {
                    if (row < _dataTable.Rows.Count && col < _dataTable.Columns.Count)
                    {
                        var rowView = _dataTable.DefaultView[row];
                        var column = excelDataGrid.Columns[col];
                        excelDataGrid.SelectedCells.Add(new DataGridCellInfo(rowView, column));
                    }
                }
            }
        }

        #endregion 辅助方法

        #region 键盘事件（Ctrl+C/V）

        private void ExcelDataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+C 复制
            if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                CopySelectedCells();
                e.Handled = true;
            }

            // Ctrl+V 粘贴
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                PasteToSelectedCells();
                e.Handled = true;
            }
        }

        #endregion 键盘事件（Ctrl+C/V）

        #region 初始化数据源

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 初始化DataTable
            _dataTable = new DataTable("ExcelStyleTable");

            // 添加初始列
            for (int i = 0; i < 5; i++)
            {
                _dataTable.Columns.Add($"列{i + 1}", typeof(object));
            }

            // 添加初始行
            for (int i = 0; i < 10; i++)
            {
                DataRow row = _dataTable.NewRow();
                for (int j = 0; j < _dataTable.Columns.Count; j++)
                {
                    row[j] = $"{i + 1}-{j + 1}"; // 初始值：行-列
                }
                _dataTable.Rows.Add(row);
            }

            // 绑定数据源
            excelDataGrid.ItemsSource = _dataTable.DefaultView;
        }

        #endregion 初始化数据源
    }
}