///
/// Author: YanzheZhang
/// Date: 26/1/2018
/// Desc:
/// 
/// Revision History:
/// -----------------------------------
///   Author:
///   Date:
///   Desc:
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using GIS.HPU.ZYZ.SHP.Util;
using System.Text.RegularExpressions;

namespace GIS.HPU.ZYZ.SHP.SHP
{
    /// <summary>
    /// 坐标文件的坐标系
    /// 目前支持GCS_WGS_1984、WGS_1984_Albers两种
    /// </summary>
    public enum ShapeProject 
    {
        /// <summary>
        /// 未知坐标系
        /// </summary>
        NULL=-1,
        /// <summary>
        /// wgs84地理坐标系
        /// </summary>
        GCS_WGS_1984=0,
        /// <summary>
        /// wgs84 albers投影坐标系
        /// </summary>
        WGS_1984_Albers=1
    }

    /// <summary>
    /// 记录几何类型
    /// </summary>
    public enum ShapeType 
    { 
        /// <summary>
        /// 表示这个Shapefile文件不含坐标
        /// </summary>
        NullShape  =0,
     	/// <summary>
        /// 表示Shapefile文件记录的是点状目标，但不是多点
     	/// </summary>
        Point      =1, 
        /// <summary>
        /// 表示Shapefile文件记录的是线状目标
        /// </summary>
        PolyLine   =3,
        /// <summary>
        /// 表示Shapefile文件记录的是面状目标
        /// </summary>
        Polygon    =5,
        /// <summary>
        /// 表示Shapefile文件记录的是多点，即点集合
        /// </summary>
        MultiPoint =8,
        /// <summary>
        /// 表示Shapefile文件记录的是三维点状目标
        /// </summary>
        PointZ     =11,
        /// <summary>
        /// 表示Shapefile文件记录的是三维线状目标
        /// </summary>
        PolyLineZ  =13,
        /// <summary>
        /// 表示Shapefile文件记录的是三维面状目标
        /// </summary>
        PolygonZ   =15,
        /// <summary>
        /// 表示Shapefile文件记录的是三维点集合目标
        /// </summary>
        MultiPointZ=18,
        /// <summary>
        /// 表示含有Measure值的点状目标
        /// </summary>
        PointM     =21,
        /// <summary>
        /// 表示含有Measure值的线状目标
        /// </summary>
        PolyLineM  =23,
        /// <summary>
        /// 表示含有Measure值的面状目标
        /// </summary>
        PolygonM   =25,
        /// <summary>
        /// 表示含有Measure值的多点目标
        /// </summary>
        MultiPointM=28,
        /// <summary>
        /// 表示复合目标
        /// </summary>
        MultiPatch =31 			 
    }

    /// <summary>
    /// 记录几何类型基类
    /// 每条记录有相应的文件头RecordNum DataLength ShapeType
    /// </summary>
    public class BaseShape
    {
        /// <summary>
        /// 记录在文件中的编号 索引从1开始
        /// 相当于featureid 第几个记录
        /// </summary>
        public ulong RecordNum;
        /// <summary>
        /// 记录长度
        /// shp文件标准：表示坐标文件中的对应记录的长度。
        /// 以字为单位，一字等于2字节，等于16位  
        /// 长度不包括每条记录的RecordNum和DataLength共8字节4位
        /// </summary>
        public int DataLength;
        /// <summary>
        /// 记录的几何类型
        /// </summary>
        public ShapeType GeoType;

        /// <summary>
        /// 将GeoShape转为wkt字符串
        /// </summary>
        /// <returns></returns>
        public virtual string ExportWKT()
        {
            return null;
        }
        /// <summary>
        /// 将wkt转为字符串GeoShape
        /// </summary>
        public virtual void TransFormWKT(string wkt)
        { 
        }
        /// <summary>
        /// 投影坐标系转地理坐标系
        /// </summary>
        public virtual void CoordTransPro2Geo() 
        {
        }
        /// <summary>
        /// 地理坐标系转投影坐标系
        /// </summary>
        public virtual void CoordTransGeo2Pro()
        {  
        }
    }

    /// <summary>
    /// 点
    /// </summary>
    public class EVPoint:BaseShape
    {
        public double X;
        public double Y;

        public EVPoint() { }
        public EVPoint(double _x,double _y)
        {
            this.X = _x;
            this.Y = _y;
        }
        /// <summary>
        /// 导出wkt
        /// </summary>
        /// <returns></returns>
        public override string ExportWKT()
        {
            return "POINT("+X+" "+Y+")";
        }
        /// <summary>
        /// 导入wkt
        /// </summary>
        /// <param name="wkt"></param>
        public override void TransFormWKT(string wkt)
        {
            if (wkt.Contains("POINT")) {
                Regex coordinateGroupPattern = new Regex("[0-9.]+ [0-9., ]+");
                MatchCollection coordinateGroupMatch = coordinateGroupPattern.Matches(wkt);
                List<string> value = new List<string>();
                //如果大于0说明正常
                if (coordinateGroupMatch.Count != 0)
                {
                    char[] scpoint = { ' ' };
                    string[] xy = coordinateGroupMatch[0].Value.Split(scpoint, StringSplitOptions.RemoveEmptyEntries);//coordinateGroupMatch[0].Value.Split(',');
                    if (xy.Length >= 2)//是2或者三  
                    {
                        X = Convert.ToDouble(xy[0]);
                        Y = Convert.ToDouble(xy[1]);
                    }

                }
            }
            this.GeoType = ShapeType.Point;
            //this.RecordNum //放在外围控制 
            //记录长度 每条长度不算RecorderNum和ContentLength的4字 点固定为10字
            this.DataLength = 10;//字数 1字=2byte=16位 
        }
        /// <summary>
        /// 投影坐标系转地理坐标系
        /// </summary>
        public override void CoordTransPro2Geo() 
        {
            EVPoint point = CoordTransform.ProToGeo(new EVPoint(X, Y));
            X = point.X;
            Y = point.Y;
        }
        /// <summary>
        /// 地理坐标系转投影坐标系
        /// </summary>
        public override void CoordTransGeo2Pro()
        {
            EVPoint point = CoordTransform.GeoToPro(new EVPoint(X, Y));
            X = point.X;
            Y = point.Y;
        }
    }
    /// <summary>
    /// 线
    /// </summary>
    public class EVPolyLine : BaseShape
    {
        public double Xmin; //边界盒
        public double Ymin; //边界盒
        public double Xmax; //边界盒
        public double Ymax; //边界盒
        public int NumParts; //部分的数目:包含子线段个数
        public int NumPoints; //点的总数目
        public ArrayList Parts; //在部分中第一个点的索引
        public ArrayList Points; //所有部分的点

        /// <summary>
        /// 导出wkt
        /// </summary>
        /// <returns></returns>
        public override string ExportWKT()
        {
            string wkt = "";
            //只考虑外环 其它舍去！
            int index = 0;
            if (Parts.Count > 1)
            {
                index = (int)Parts[1] - 1;//是否需要减1 没有验证！！！
            }
            else
            {
                index = Points.Count;
            }
            if (index >= 2)
            {
                wkt += "LINESTRING ((";
                wkt += ((EVPoint)Points[0]).X + " " + ((EVPoint)Points[0]).Y;
                for (int i = 1; i < index; i++)
                {
                    wkt += "," + ((EVPoint)Points[i]).X + " " + ((EVPoint)Points[i]).Y;
                }
                wkt += "))";
            }
            return wkt;
        }
        /// <summary>
        /// 导入wkt
        /// </summary>
        /// <param name="wkt"></param>
        public override void TransFormWKT(string wkt)
        {
            if (wkt.Contains("LINESTRING"))
            {
                //清除当前
                if (Parts != null)
                {
                    Parts.Clear();
                }
                else
                {
                    Parts = new ArrayList();
                }
                if (Points != null)
                {
                    Points.Clear();
                }
                else
                {
                    Points = new ArrayList();
                }
                Regex coordinateGroupPattern = new Regex("[0-9.]+ [0-9., ]+");
                MatchCollection coordinateGroupMatch = coordinateGroupPattern.Matches(wkt);
                List<string> value = new List<string>();
                //如果大于0说明正常 大于1说明有内环 第一个为多边形外环
                if (coordinateGroupMatch.Count != 0)
                {
                    //只考虑外环，内含舍去
                    //foreach (Match m in coordinateGroupMatch)
                    //{
                    //    value.Add(m.Value);
                    //}
                    char[] sc = { ',' };
                    char[] scpoint = { ' ' };
                    string[] cors = coordinateGroupMatch[0].Value.Split(sc, StringSplitOptions.RemoveEmptyEntries);//coordinateGroupMatch[0].Value.Split(',');
                    if (cors.Length > 0)
                    {
                        for (int i = 0; i < cors.Length; i++)
                        {
                            EVPoint point = new EVPoint();
                            string[] xy = cors[i].Split(scpoint, StringSplitOptions.RemoveEmptyEntries);//cors[i].Split(' ');
                            if (xy.Length < 2)//是2或者三
                            {
                                continue;
                            }
                            else
                            {
                                point.X = Convert.ToDouble(xy[0]);
                                point.Y = Convert.ToDouble(xy[1]);
                                Points.Add(point);
                            }
                        }
                    }
                }
            }
            if (Points.Count > 0)
            {
                Parts.Add(0);
                NumParts = 1;
                NumPoints = Points.Count;
                //求边界盒
                double[] border = BorderUtil.GetBorder(Points);
                Xmin = border[0];
                Ymin = border[1];
                Xmax = border[2];
                Ymax = border[3];

                this.GeoType = ShapeType.PolyLine;
                //this.RecordNum //放在外围控制 
                //记录长度 每条长度不算RecorderNum和ContentLength的4字  NumParts固定为1
                this.DataLength = 24 + 8 * NumPoints;//字数 1字=2byte=16位 
            }
            else
            {
                Parts.Add(0);
                NumParts = 0;
                NumPoints = 0;
                Xmin = 0;
                Ymin = 0;
                Xmax = 0;
                Ymax = 0;
                this.GeoType = ShapeType.PolyLine;
                this.DataLength = 24 + 8 * NumPoints;//字数 1字=2byte=16位 
            }
        }

        /// <summary>
        /// 投影坐标系转地理坐标系
        /// </summary>
        public override void CoordTransPro2Geo()
        {
            for (int i = 0; i < Points.Count; i++)
            {
                EVPoint point = CoordTransform.ProToGeo((EVPoint)Points[i]);
                Points.RemoveAt(i);
                Points.Insert(i, point);
            }
        }
        /// <summary>
        /// 地理坐标系转投影坐标系
        /// </summary>
        public override void CoordTransGeo2Pro()
        {
            for (int i = 0; i < Points.Count; i++) {
                EVPoint point = CoordTransform.GeoToPro((EVPoint)Points[i]);
                Points.RemoveAt(i);
                Points.Insert(i,point);
            }
            //更新边界盒
            double[] border = BorderUtil.GetBorder(Points);
            Xmin = border[0];
            Ymin = border[1];
            Xmax = border[2];
            Ymax = border[3];
        }
    }
    /// <summary>
    /// 面
    /// </summary>
    public class EVPolygon : BaseShape
    {
        public double Xmin; //边界盒
        public double Ymin; //边界盒
        public double Xmax; //边界盒
        public double Ymax; //边界盒
        public int NumParts; //部分的数目:包含子环的个数
        public int NumPoints; //点的总数目
        public ArrayList Parts; //在部分中第一个点的索引
        public ArrayList Points; //所有部分的点

        /// <summary>
        /// 导出wkt
        /// </summary>
        /// <returns></returns>
        public override string ExportWKT()
        {
            string wkt = "";
            //只考虑外环 其它舍去！
            int index = 0;
            if (Parts.Count > 1)
            {
                index = (int)Parts[1] - 1;//是否需要减1 没有验证！！！
            }
            else {
                index = Points.Count;
            }
            if (index > 2)
            {
                wkt += "POLYGON ((";
                wkt += ((EVPoint)Points[0]).X + " " + ((EVPoint)Points[0]).Y;
                for (int i = 1; i < index; i++)
                {
                    wkt += "," + ((EVPoint)Points[i]).X + " " + ((EVPoint)Points[i]).Y;
                }
                wkt += "))";
            }
            return wkt;
        }
        /// <summary>
        /// 导入wkt
        /// </summary>
        /// <param name="wkt"></param>
        public override void TransFormWKT(string wkt)
        {
            if (wkt.Contains("POLYGON"))
            {
                //清除当前
                if (Parts != null)
                {
                    Parts.Clear();
                }
                else
                {
                    Parts = new ArrayList();
                }
                if (Points != null)
                {
                    Points.Clear();
                }
                else
                {
                    Points = new ArrayList();
                }
                Regex coordinateGroupPattern = new Regex("[0-9.]+ [0-9., ]+");
                MatchCollection coordinateGroupMatch = coordinateGroupPattern.Matches(wkt);
                List<string> value = new List<string>();
                //如果大于0说明正常 大于1说明有内环 第一个为多边形外环
                if (coordinateGroupMatch.Count != 0)
                {
                    //只考虑外环，内含舍去
                    //foreach (Match m in coordinateGroupMatch)
                    //{
                    //    value.Add(m.Value);
                    //}
                    char[] sc = { ',' };
                    char[] scpoint = { ' ' };
                    string[] cors = coordinateGroupMatch[0].Value.Split(sc, StringSplitOptions.RemoveEmptyEntries);//coordinateGroupMatch[0].Value.Split(',');
                    if (cors.Length > 0)
                    {
                        for (int i = 0; i < cors.Length; i++)
                        {
                            EVPoint point = new EVPoint();
                            string[] xy = cors[i].Split(scpoint, StringSplitOptions.RemoveEmptyEntries);//cors[i].Split(' ');
                            if (xy.Length < 2)//是2或者三
                            {
                                continue;
                            }
                            else
                            {
                                point.X = Convert.ToDouble(xy[0]);
                                point.Y = Convert.ToDouble(xy[1]);
                                Points.Add(point);
                            }
                        }
                    }
                }
            }
            if (Points.Count > 0)
            {
                Parts.Add(0);
                NumParts = 1;
                NumPoints = Points.Count;
                //求边界盒
                double[] border = BorderUtil.GetBorder(Points);
                Xmin = border[0];
                Ymin = border[1];
                Xmax = border[2];
                Ymax = border[3];

                this.GeoType = ShapeType.Polygon;
                //this.RecordNum //放在外围控制 
                //记录长度 每条长度不算RecorderNum和ContentLength的4字  NumParts固定为1
                this.DataLength = 24 + 8 * NumPoints;//字数 1字=2byte=16位 
            }
            else
            {
                Parts.Add(0);
                NumParts = 0;
                NumPoints = 0;
                Xmin = 0;
                Ymin = 0;
                Xmax = 0;
                Ymax = 0;
                this.GeoType = ShapeType.Polygon;
                this.DataLength = 24 + 8 * NumPoints;//字数 1字=2byte=16位 
            }
        }

        /// <summary>
        /// 投影坐标系转地理坐标系
        /// </summary>
        public override void CoordTransPro2Geo()
        {
            for (int i = 0; i < Points.Count; i++)
            {
                EVPoint point = CoordTransform.ProToGeo((EVPoint)Points[i]);
                Points.RemoveAt(i);
                Points.Insert(i, point);
            }
        }
        /// <summary>
        /// 地理坐标系转投影坐标系
        /// </summary>
        public override void CoordTransGeo2Pro()
        {
            for (int i = 0; i < Points.Count; i++)
            {
                EVPoint point = CoordTransform.GeoToPro((EVPoint)Points[i]);
                Points.RemoveAt(i);
                Points.Insert(i, point);
            }
            //更新边界盒
            double[] border = BorderUtil.GetBorder(Points);
            Xmin = border[0];
            Ymin = border[1];
            Xmax = border[2];
            Ymax = border[3];
        }
    }
}
