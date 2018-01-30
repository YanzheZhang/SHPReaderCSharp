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

namespace GIS.HPU.ZYZ.SHP.SHP
{
    /// <summary>
    /// shp文件一条记录的数据结构
    /// 
    /// </summary>
    public class ShpData
    {
        /// <summary>
        /// shape对象
        /// 目前支持处理EVPoint、EVPolyLine、EVPolygon三类
        /// </summary>
        private BaseShape mGeoShape;
        /// <summary>
        /// 几何对象的WKT字符串
        /// </summary>
        private string mWKTStr;
        /// <summary>
        /// 标记geo是否转换为wkt
        /// </summary>
        private bool mGeo2WKT = false;
        /// <summary>
        /// 标记wkt是否转换为geo
        /// </summary>
        private bool mWKT2Geo = false;

        /// <summary>
        /// shape对象
        /// 目前支持处理EVPoint、EVPolyLine、EVPolygon三类
        /// </summary>
        public BaseShape GeoShape
        {
            get { return mGeoShape; }
            set { mGeoShape = value; }
        }
        /// <summary>
        /// 几何对象的WKT字符串
        /// </summary>
        public string WKTStr
        {
            get {
                if (!mGeo2WKT) {
                    mWKTStr=mGeoShape.ExportWKT();
                    mGeo2WKT = true;
                }
                return mWKTStr; 
            }
            set {
                mWKTStr = value; 
                if (!mWKT2Geo) {
                    //mGeoShape.TransFormWKT(mWKTStr);
                    if (mWKTStr.Contains("POINT"))
                    {
                        mGeoShape = new EVPoint();
                        mGeoShape.TransFormWKT(mWKTStr);
                    }
                    else if (mWKTStr.Contains("LINESTRING"))
                    {
                        mGeoShape = new EVPolyLine();
                        mGeoShape.TransFormWKT(mWKTStr);
                    }
                    else if (mWKTStr.Contains("POLYGON"))
                    {
                        mGeoShape = new EVPolygon();
                        mGeoShape.TransFormWKT(mWKTStr);
                    }
                    mWKT2Geo = true;
                }
            }
        }

        /// <summary>
        /// 投影坐标系转地理坐标系
        /// </summary>
        public void CoordTransPro2Geo()
        {
            mGeoShape.CoordTransPro2Geo();
        }
        /// <summary>
        /// 地理坐标系转投影坐标系
        /// </summary>
        public void CoordTransGeo2Pro()
        {
            mGeoShape.CoordTransGeo2Pro();
        }
    }
}
