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
        public Span()
        {
            lineA = new VerticalLine();
            lineB = new VerticalLine();
            this.Reset();
        }

        private VerticalLine lineA;


        public VerticalLine LineA

        {
            set
            {
                if (false == isLineASet)
                {
                    lineA = value;
                    lineA.Value = 0;
                }
            }
            get
            {
                return lineA;
            }
        }
        private VerticalLine lineB;
        public VerticalLine LineB
        {
            set 
            {
                if (false == isLineBSet)
                {
                    lineA = value;
                    isLineBSet = false;
                }
            }
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
