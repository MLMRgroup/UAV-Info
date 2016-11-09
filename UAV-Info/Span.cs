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
        SolidColorBrush brush = new SolidColorBrush(System.Windows.Media.Colors.Red);
        public Span()
        {
            lineA = new VerticalLine();
            lineB = new VerticalLine();
            lineA.Stroke = brush;
            lineA.Stroke = brush;
            this.Reset();
        }

        private VerticalLine lineA;
        public void SetLineA(double value)
        {
            if(false == isLineASet)
            {
                lineA.Value = value;
                isLineASet = true;
            }
        }
        public VerticalLine LineA
        {
            get
            {
                return lineA;
            }
        }

        private VerticalLine lineB;
        public void SetLineB(double value)
        {
            if (false == isLineBSet)
            {
                lineB.Value = value;
                isLineBSet = true;
            }
        }
        public VerticalLine LineB
        {
            get
            {
                return lineB;
            }
        }

        private Boolean isLineASet;
        private Boolean isLineBSet;
        public Boolean IsSet
        {
            get 
            {
                return isLineASet && isLineBSet; 
            }
        }
        public void Reset()
        {
            isLineASet = false;
            isLineBSet = false;
            lineA.Stroke = null;
            lineB.Stroke = null;
            lineA.Value = 0;
            lineB.Value = 0;
        }
    }
}
