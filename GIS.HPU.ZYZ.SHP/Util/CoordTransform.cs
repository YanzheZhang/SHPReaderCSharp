///
/// Author: YanzheZhang
/// Date: 26/1/2018
/// Desc:
/// 
/// Revision History:
/// -----------------------------------
///   Author:LiuJiao
///   Date:---
///   Desc:---
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjNet.CoordinateSystems;
using ProjNet.Converters.WellKnownText;
using GIS.HPU.ZYZ.SHP.SHP;
using ProjNet.CoordinateSystems.Transformations;

namespace GIS.HPU.ZYZ.SHP.Util
{
    /// <summary>
    /// 该类为平面坐标与地理坐标相互转换类
    /// </summary>
    public class CoordTransform
    {
        /// <summary>
        /// 该值指示地理坐标系统
        /// </summary>
        private static IGeographicCoordinateSystem GeoCS;
        /// <summary>
        /// 该值指示投影坐标系统
        /// </summary>
        private static IProjectedCoordinateSystem ProCS;
        /// <summary>
        /// 是否初始化路径
        /// </summary>
        private static bool isInitPath = false;

        /// <summary>
        /// 创建坐标转换工具
        /// </summary>
        public static void InitPath()
        {
            string basePath = "";
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetEntryAssembly();
            if (asm == null)
                basePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            else
                basePath = System.AppDomain.CurrentDomain.BaseDirectory;// System.Windows.Forms.Application.StartupPath;
            string geoFile = System.IO.Path.Combine(basePath, "Project\\geopro.prj");
            string proFile = System.IO.Path.Combine(basePath, "Project\\abers.prj");
            using (System.IO.StreamReader sr = new System.IO.StreamReader(geoFile))
            {
                string geowkt = sr.ReadLine();
                GeoCS = CoordinateSystemWktReader.Parse(geowkt) as IGeographicCoordinateSystem;
            }
            using (System.IO.StreamReader sr = new System.IO.StreamReader(proFile))
            {
                string prowkt = sr.ReadLine();
                ProCS = CoordinateSystemWktReader.Parse(prowkt) as IProjectedCoordinateSystem;
            }
            isInitPath = true;
        }
        /// <summary>
        /// 创建坐标转换工具
        /// </summary>
        /// <param name="geoToPrj">该值指示地理-投影的坐标文件</param>
        /// <param name="prjToGeo">该值指示投影-地理的坐标文件</param>
        public static void ChangePath(string geoToPrj, string prjToGeo)
        {
            using (System.IO.StreamReader sr = new System.IO.StreamReader(geoToPrj))
            {
                string geowkt = sr.ReadLine();
                GeoCS = CoordinateSystemWktReader.Parse(geowkt) as IGeographicCoordinateSystem;
            }
            using (System.IO.StreamReader sr = new System.IO.StreamReader(prjToGeo))
            {
                string prowkt = sr.ReadLine();
                ProCS = CoordinateSystemWktReader.Parse(prowkt) as IProjectedCoordinateSystem;
            }
            isInitPath = true;
        }
        /// <summary>
        /// 该方法用于将投影坐标转换为地理坐标
        /// </summary>
        /// <param name="position">该值指示投影坐标</param>
        /// <returns>该值指示地理坐标</returns>
        public static EVPoint ProToGeo(EVPoint position)
        {
            if (!isInitPath) {
                InitPath();
            }
            double[] d = new double[3] { position.X, position.Y, 0 };
            EVPoint v = CoordianteConvert(new EVPoint(d[0], d[1]), ProCS, GeoCS);
            return v;
        }
        /// <summary>
        /// 该方法用于将地理坐标转换成投影坐标
        /// </summary>
        /// <param name="geoPosition">地理坐标</param>
        /// <returns>投影坐标</returns>
        public static EVPoint GeoToPro(EVPoint geoPosition)
        {
            if (!isInitPath)
            {
                InitPath();
            }
            double[] d = new double[3] { geoPosition.X, geoPosition.Y, 0 };
            EVPoint v = CoordianteConvert(new EVPoint(d[0], d[1]), GeoCS, ProCS);
            return v;
        }
        /// <summary>
        /// 该方法用于将某一坐标从一个坐标系统转换成另一个坐标系统的坐标
        /// </summary>
        /// <param name="input">该值指示待转换坐标</param>
        /// <param name="fromCS">该值指示待转换坐标的坐标系统</param>
        /// <param name="toCS">该值指示目标坐标系统</param>
        /// <returns>该值指示转换后的结果</returns>
        private static EVPoint CoordianteConvert(EVPoint input, ICoordinateSystem fromCS, ICoordinateSystem toCS)
        {
            try
            {
                List<double[]> linput = new List<double[]>();
                linput.Add(new double[] { input.X, input.Y, 0 });
                CoordinateTransformationFactory ctfac = new CoordinateTransformationFactory();
                ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(fromCS, toCS);
                List<double[]> loutput = trans.MathTransform.TransformList(linput);
                EVPoint output = new EVPoint();
                double[] dout = loutput[0];
                output.X = dout[0];
                output.Y = dout[1];
                //output.Z = dout[2];
                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine("坐标转换错误：CoordinateConvert(...)", ex.ToString());
                return null;
            }
        }
    }
}
