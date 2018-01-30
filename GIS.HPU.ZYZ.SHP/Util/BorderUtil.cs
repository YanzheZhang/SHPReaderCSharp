using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using GIS.HPU.ZYZ.SHP.SHP;

namespace GIS.HPU.ZYZ.SHP.Util
{
    /// <summary>
    /// 边界盒操作类
    /// </summary>
    public class BorderUtil
    {
        /// <summary>
        /// 获取坐标串的边界盒
        /// </summary>
        /// <param name="points">evpoint类型</param>
        /// <returns></returns>
        public static double[] GetBorder(ArrayList points)
        {
            double[] border = new double[4];
            double Xmin = ((EVPoint)points[0]).X;
            double Ymin = ((EVPoint)points[0]).Y;
            double Xmax = ((EVPoint)points[0]).X;
            double Ymax = ((EVPoint)points[0]).Y;
            foreach (EVPoint pont in points)
            {
                if (Xmin > pont.X)
                {
                    Xmin = pont.X;
                }
                if (Xmax < pont.X)
                {
                    Xmax = pont.X;
                }
                if (Ymin > pont.Y)
                {
                    Ymin = pont.Y;
                }
                if (Ymax < pont.Y)
                {
                    Ymax = pont.Y;
                }
            }
            border[0] = Xmin;
            border[1] = Ymin;
            border[2] = Xmax;
            border[3] = Ymax;
            return border;
        }
    }
}
