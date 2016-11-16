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
        //markBrush：红色刷子  unmarkBrush：透明刷子
        SolidColorBrush markBrush = new SolidColorBrush(System.Windows.Media.Colors.Red);
        SolidColorBrush unmarkBrush = new SolidColorBrush(System.Windows.Media.Colors.Transparent);

        private Boolean isLineASet;
        private Boolean isLineBSet;
        //每条直线可以有多个自身复制，由Span类统一管理，存储在各自的List中
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
                lineA[lineA.Count - 1].Stroke = unmarkBrush;
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
                lineB[lineB.Count - 1].Stroke = unmarkBrush;
                return lineB[lineB.Count - 1];
            }
        }
        //得到A基准线的索引值
        public double valueOfLineA
        {
            get
            {
                if (lineA.Count >= 1)
                {
                    return lineA[0].Value;
                }
                return 0;
            }
        }
        //得到B基准线的索引值
        public double valueOfLineB
        {
            get
            {
                if (lineB.Count >= 1)
                {
                    return lineB[0].Value;
                }
                return 0;
            }
        }
        //改变当前基准线的值  优先设置A基准线  设置之后的基准线以红色显示出来
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
                lineA[i].Stroke = unmarkBrush;
            }
            for (int i = 0; i < lineB.Count; i++)
            {
                lineB[i].Value = 0;
                lineB[i].Stroke = unmarkBrush;
            }
            isLineASet = false;
            isLineBSet = false;
        }
    }
}
