using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ProcessManageWPF.Visitor;

namespace ProcessManageWPF
{
    /// <summary>
    /// 内存分段视图
    /// </summary>
    public partial class SegmentView : UserControl
    {
        public Style OccupyStyle => (Style)FindResource("OccupyStyle");
        public Style AvailableStyle => (Style) FindResource("AvailableStyle");

        private IEnumerable<MemoryBlockVisitor> _memoryBlocks;

        public IEnumerable<MemoryBlockVisitor> MemoryBlocks
        {
            get => _memoryBlocks;
            set => ResetView((_memoryBlocks = value).ToArray());
        }

        public SegmentView()
        {
            InitializeComponent();
        }

        private void ResetView(MemoryBlockVisitor[] visitors)
        {
            var count = visitors.Length;
            if (MainGrid.ColumnDefinitions.Count < count)
            {
                for (var index = MainGrid.ColumnDefinitions.Count; index < count; index++)
                {
                    // 新建标签
                    var label = new Label { Template = (ControlTemplate)FindResource("Segment") };
                    // label.DataContext = visitors[index];
                    // 新建列
                    var column = new ColumnDefinition();
                    label.SetValue(Grid.ColumnProperty, MainGrid.ColumnDefinitions.Count);
                    MainGrid.Children.Add(label);
                    MainGrid.ColumnDefinitions.Add(column);
                }
            }
            else
            {
                // 删除多余项
                if (MainGrid.ColumnDefinitions.Count > count)
                {
                    MainGrid.ColumnDefinitions.RemoveRange(count, MainGrid.ColumnDefinitions.Count - count);
                    MainGrid.Children.RemoveRange(count, MainGrid.Children.Count - count);
                }
            }
            // 设置数据
            for (var i = 0; i < count; i++)
            {
                var control = (Control)MainGrid.Children[i];
                control.DataContext = visitors[i];
                control.Style = visitors[i].Occupied ? OccupyStyle : AvailableStyle;
                MainGrid.ColumnDefinitions[i].Width = new GridLength(visitors[i].Length, GridUnitType.Star);
            }


        }
    }
}
