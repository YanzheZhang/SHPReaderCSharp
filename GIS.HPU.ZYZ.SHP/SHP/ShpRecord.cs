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
using System.IO;
using System.Collections;
using GIS.HPU.ZYZ.SHP.Util;

namespace GIS.HPU.ZYZ.SHP.SHP
{
    /// <summary>
    /// shp文件对应的记录内容
    /// 提供文件记录的读写操作
    /// </summary>
    public class ShpRecord
    {
        /// <summary>
        /// Header提供shp文件的详细信息
        /// 类型、大小、包围盒等
        /// </summary>
        private ShpHeader mHeader = null;
        /// <summary>
        /// shp文件记录字典
        /// key：shp的记录索引--从1开始（featureid）
        /// value：该条记录的具体内容：ShpGeoData类型
        /// </summary>
        private Dictionary<ulong, ShpData> mRecordDic = null;

        #region
        /// <summary>
        /// Header提供shp文件的详细信息
        /// 类型、大小、包围盒等
        /// </summary>
        public ShpHeader Header
        {
            get { return mHeader; }
            set { mHeader = value; }
        }
        /// <summary>
        /// shp文件记录字典
        /// key：shp的记录索引--从1开始（featureid）
        /// value：该条记录的具体内容：ShpData类型
        /// </summary>
        public Dictionary<ulong, ShpData> RecordDic
        {
            get { return mRecordDic; }
            //set { mRecordDic = value; }
        }
        #endregion

        /// <summary>
        /// 构造时传入已经实例化的ShpHeader对象
        /// </summary>
        /// <param name="oHeader"></param>
        public ShpRecord(ShpHeader oHeader) 
        {
            mHeader = oHeader;
        }

        /// <summary>
        /// 读取record数据
        /// 当读取时文件流位置在第一个记录处
        /// </summary>
        /// <param name="reader"></param>
        public void Read(BinaryReader reader) 
        {
            //如果header为空先读取header
            if (mHeader == null) {
                mHeader.Read(reader);
            }
            if (mRecordDic != null)
            {
                mRecordDic.Clear();
            }
            else {
                mRecordDic = new Dictionary<ulong, ShpData>();
            }
            //目前只考虑点线面三种类型
            switch (mHeader.GeoType) {
                case ShapeType.Point:
                    while (reader.PeekChar() != -1)
                    {
                        EVPoint point = new EVPoint();
                        uint RecordNum = reader.ReadUInt32();
                        int DataLength = reader.ReadInt32();
                        ulong uRecordNum = ByteTransUtil.big2little((int)RecordNum);//读取第i个记录
                        ulong uDataLength = ByteTransUtil.big2little(DataLength);//以字为单位，一字等于2字节，等于16位 长度不包括uRecordNum和uDataLength 
                        int geoType = reader.ReadInt32();//每一个对象的类型 没有用！
                        point.RecordNum = uRecordNum;
                        point.DataLength = uDataLength;
                        point.GeoType = ShapeType.Point;
                        point.X = reader.ReadDouble();
                        point.Y = reader.ReadDouble();//
                        ShpData item = new ShpData();
                        item.GeoShape = point;
                        mRecordDic.Add(uRecordNum, item);
                    }
                    break;
                case ShapeType.PolyLine:
                    while (reader.PeekChar() != -1)
                    {
                        EVPolyLine polyline = new EVPolyLine();
                        polyline.Parts = new ArrayList();
                        polyline.Points = new ArrayList();

                        uint RecordNum = reader.ReadUInt32();//读取第i个记录 从1开始算
                        ulong uRecordNum = ByteTransUtil.big2little((int)RecordNum);
                        int DataLength = reader.ReadInt32();//以字为单位，一字等于2字节，等于16位
                        ulong uDataLength = ByteTransUtil.big2little(DataLength);
                        int geoType = reader.ReadInt32();//每一个对象的类型 没有用

                        polyline.RecordNum = uRecordNum;
                        polyline.DataLength = uDataLength;
                        polyline.GeoType = ShapeType.PolyLine;
                        polyline.Box1 = reader.ReadDouble();
                        polyline.Box2 = reader.ReadDouble();
                        polyline.Box3 = reader.ReadDouble();
                        polyline.Box4 = reader.ReadDouble();
                        polyline.NumParts = reader.ReadInt32();
                        polyline.NumPoints = reader.ReadInt32();
                        for (int i = 0; i < polyline.NumParts; i++)
                        {
                            int parts = new int();
                            parts = reader.ReadInt32();
                            polyline.Parts.Add(parts);
                        }
                        for (int j = 0; j < polyline.NumPoints; j++)
                        {

                            EVPoint pointtemp = new EVPoint();
                            pointtemp.X = reader.ReadDouble();
                            pointtemp.Y = reader.ReadDouble();
                            polyline.Points.Add(pointtemp);
                        }
                        ShpData item = new ShpData();
                        item.GeoShape = polyline;
                        mRecordDic.Add(uRecordNum, item);
                    }
                    break;
                case ShapeType.Polygon:
                    while (reader.PeekChar() != -1)
                    {
                        EVPolygon polygon = new EVPolygon();
                        polygon.Parts = new ArrayList();
                        polygon.Points = new ArrayList();

                        uint RecordNum = reader.ReadUInt32();
                        int DataLength = reader.ReadInt32();
                        ulong uRecordNum = ByteTransUtil.big2little((int)RecordNum);//读取第i个记录
                        ulong uDataLength = ByteTransUtil.big2little(DataLength);//以字为单位，一字等于2字节，等于16位
                        int geoType = reader.ReadInt32();

                        polygon.RecordNum = uRecordNum;
                        polygon.DataLength = uDataLength;
                        polygon.GeoType = ShapeType.Polygon;
                        polygon.Box1 = reader.ReadDouble();
                        polygon.Box2 = reader.ReadDouble();
                        polygon.Box3 = reader.ReadDouble();
                        polygon.Box4 = reader.ReadDouble();
                        polygon.NumParts = reader.ReadInt32();
                        polygon.NumPoints = reader.ReadInt32();
                        for (int j = 0; j < polygon.NumParts; j++)
                        {
                            int parts = new int();
                            parts = reader.ReadInt32();
                            polygon.Parts.Add(parts);
                        }
                        for (int j = 0; j < polygon.NumPoints; j++)
                        {
                            EVPoint pointtemp = new EVPoint();
                            pointtemp.X = reader.ReadDouble();
                            pointtemp.Y = reader.ReadDouble();
                            polygon.Points.Add(pointtemp);
                        }
                        ShpData item = new ShpData();
                        item.GeoShape = polygon;
                        mRecordDic.Add(uRecordNum, item); 
                    }
                    break;
            }
        }


        /// <summary>
        /// 获取记录数量
        /// </summary>
        /// <returns></returns>
        public int GetRecordCount() {
            return mRecordDic!=null ? mRecordDic.Count : 0;
        }
        /// <summary>
        /// 投影坐标系转地理坐标系
        /// </summary>
        public void CoordTransPro2Geo()
        {
            foreach (var item in mRecordDic) 
            {
                item.Value.CoordTransPro2Geo();
            }
        }
        /// <summary>
        /// 地理坐标系转投影坐标系
        /// </summary>
        public void CoordTransGeo2Pro()
        {
            foreach (var item in mRecordDic)
            {
                item.Value.CoordTransGeo2Pro();
            }
        }

    }
}
