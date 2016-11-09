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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts.Navigation;
using System.Collections.ObjectModel;

namespace UAV_Info
{
    class Span
    {
        SolidColorBrush markBrush = new SolidColorBrush(System.Windows.Media.Colors.Red);
        SolidColorBrush tranpBrush = new SolidColorBrush(System.Windows.Media.Colors.Transparent);

        private Boolean isLineASet;
        private Boolean isLineBSet;
        private List<VerticalLine> lineA;
        private List<VerticalLine> lineB;
        public Span()
        {
            lineA = new List<VerticalLine>();
            lineB = new List<VerticalLine>();
            this.Reset();
        }
        //获取一个A组基准线实例
        public VerticalLine LineA
        {
            get
            {
                lineA.Add(new VerticalLine());
                lineA[lineA.Count - 1].Value = 0;
                lineA[lineA.Count - 1].Stroke = tranpBrush;
                return lineA[lineA.Count-1];
            }
        }
        //获取一个B组基准线实例
        public VerticalLine LineB
        {
            get
            {
                lineB.Add(new VerticalLine());
                lineB[lineB.Count - 1].Value = 0;
                lineB[lineB.Count - 1].Stroke = tranpBrush;
                return lineB[lineB.Count - 1];
            }
        }
        //改变当前基准线的值
        public void AddLine(double value)
        {
            if (false == isLineASet)
            {
                for (int i = 0; i < lineA.Count; i++)
                {
                    lineA[i].Value = value;
                    lineA[i].Stroke = markBrush;
                }
                isLineASet = true;
            }
            else if (false == isLineBSet)
            {
                for (int i = 0; i < lineB.Count; i++)
                {
                    lineB[i].Value = value;
                    lineB[i].Stroke = markBrush;
                }
                isLineBSet = true;
            }
        }
        //检查基准线是否都设置成功
        public Boolean IsSet
        {
            get 
            {
                return isLineASet && isLineBSet; 
            }
        }
        //重置基准线
        public void Reset()
        {
            for (int i = 0; i < lineA.Count; i++)
            {
                lineA[i].Value = 0;
                lineA[i].Stroke = tranpBrush;
            }
            for (int i = 0; i < lineB.Count; i++)
            {
                lineB[i].Value = 0;
                lineB[i].Stroke = tranpBrush;
            }
            isLineASet = false;
            isLineBSet = false;
        }
    }
}
