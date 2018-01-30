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
using GIS.HPU.ZYZ.SHP.SHX;

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
        /// <summary>
        /// 是否获取wkt信息，标记是否可以写入record
        /// </summary>
        private bool isGetWKTInfo = false;
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
        /// <summary>
        /// 是否获取wkt信息，标记是否可以写入record
        /// 在调用Write时先判断此标记
        /// </summary>
        public bool IsGetWKTInfo
        {
            get { return isGetWKTInfo; }
            //set { isGetWKTInfo = value; }
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
                        //每一条的记录长度没有考虑RecordNum、DataLength 所以比实际长度少4字
                        int uDataLength = (int)ByteTransUtil.big2little(DataLength);//以字为单位，一字等于2字节，等于16位 长度不包括uRecordNum和uDataLength 
                        int geoType = reader.ReadInt32();//每一个对象的类型 没有用！
                        point.RecordNum = uRecordNum;
                        point.DataLength = uDataLength;//固定10字 没有考虑RecordNum、DataLength 比实际长度少4字
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
                        //每一条的记录长度没有考虑RecordNum、DataLength 所以比实际长度少4字
                        int uDataLength = (int)ByteTransUtil.big2little(DataLength);
                        int geoType = reader.ReadInt32();//每一个对象的类型 没有用

                        polyline.RecordNum = uRecordNum;
                        polyline.DataLength = uDataLength;//没有考虑RecordNum、DataLength 比实际长度少4字
                        polyline.GeoType = ShapeType.PolyLine;

                        polyline.Xmin = reader.ReadDouble();
                        polyline.Ymin = reader.ReadDouble();
                        polyline.Xmax = reader.ReadDouble();
                        polyline.Ymax = reader.ReadDouble();
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
                        //每一条的记录长度没有考虑RecordNum、DataLength 所以比实际长度少4字
                        //以字为单位，一字等于2字节，等于16位
                        int uDataLength = (int)ByteTransUtil.big2little(DataLength);
                        int geoType = reader.ReadInt32();

                        polygon.RecordNum = uRecordNum;
                        polygon.DataLength = uDataLength;
                        polygon.GeoType = ShapeType.Polygon;
                        polygon.Xmin = reader.ReadDouble();
                        polygon.Ymin = reader.ReadDouble();
                        polygon.Xmax = reader.ReadDouble();
                        polygon.Ymax = reader.ReadDouble();
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
        /// 从wkt获取shp文件信息，包括头文件和记录
        /// </summary>
        /// <param name="wktList"></param>
        public void GetWKTInfo(List<string> wktList) 
        {
            if (mRecordDic != null)
            {
                mRecordDic.Clear();
            }
            else {
                mRecordDic = new Dictionary<ulong, ShpData>();
            }
            
            if (wktList!=null && wktList.Count > 0) {
                for(int i=0;i<wktList.Count;i++){
                     ShpData data = new ShpData();
                     data.WKTStr=wktList[i];
                     data.CoordTransGeo2Pro();//从数据库读出是地理坐标系 需要转换为投影坐标系
                     data.GeoShape.RecordNum = (ulong)(i + 1);
                     mRecordDic.Add((ulong)(i + 1), data);
                }
            }
            //根据RecordDic求header
            TransHeader();
            isGetWKTInfo = true;
        }
        /// <summary>
        /// 写入record记录
        /// 先判断是否获取wkt信息以及写入header
        /// 写入的同时 计算shx的记录内容
        /// </summary>
        /// <param name="writer"></param>
        /// <returns>返回shx的文件内容</returns>
        public Dictionary<ulong, ShxData> Write(BinaryWriter writer) 
        {
            Dictionary<ulong, ShxData> shxDic = new Dictionary<ulong, ShxData>();
            if (isGetWKTInfo) {
                if (writer.BaseStream.Position == 0)
                {
                    mHeader.Write(writer);
                }
                int offset = 50;//跳过头文件
                switch (mHeader.GeoType)
                {
                    case ShapeType.Point:
                        for (int i = 0; i < mRecordDic.Count; i++)
                        {
                            EVPoint point = mRecordDic[(ulong)(i + 1)].GeoShape as EVPoint;
                            ShxData shx = new ShxData();
                            shx.ContentLength = point.DataLength;
                            shx.Offset = offset;
                            offset += shx.ContentLength + 4;
                            shxDic.Add(point.RecordNum, shx);
                            //写入shp
                            byte[] lbtRecorderNum = BitConverter.GetBytes(point.RecordNum);
                            byte[] bbtRecorderNum = ByteTransUtil.little2big(lbtRecorderNum);
                            writer.Write(bbtRecorderNum);

                            byte[] lbtDataLength = BitConverter.GetBytes(point.DataLength);
                            byte[] bbtDataLength = ByteTransUtil.little2big(lbtDataLength);
                            writer.Write(bbtDataLength);
                            writer.Write((int)ShapeType.Point);
                            writer.Write(point.X);
                            writer.Write(point.Y);
                        }
                       
                        break;
                    case ShapeType.PolyLine:
                        for (int i = 0; i < mRecordDic.Count; i++)
                        {
                            EVPolyLine line = mRecordDic[(ulong)(i + 1)].GeoShape as EVPolyLine;
                            ShxData shx = new ShxData();
                            shx.ContentLength = line.DataLength;
                            shx.Offset = offset;
                            offset += shx.ContentLength + 4;
                            shxDic.Add(line.RecordNum,shx);
                            //写入shp
                            byte[] lbtRecorderNum = BitConverter.GetBytes(line.RecordNum);
                            byte[] bbtRecorderNum = ByteTransUtil.little2big(lbtRecorderNum);
                            writer.Write(bbtRecorderNum);

                            byte[] lbtDataLength = BitConverter.GetBytes(line.DataLength);
                            byte[] bbtDataLength = ByteTransUtil.little2big(lbtDataLength);
                            writer.Write(bbtDataLength);

                            writer.Write((int)line.GeoType);
                            writer.Write(line.Xmin);
                            writer.Write(line.Ymin);
                            writer.Write(line.Xmax);
                            writer.Write(line.Ymax);

                            writer.Write(line.NumParts);
                            writer.Write(line.NumPoints);

                            for (int j = 0; j < line.NumParts; j++)
                            {
                                writer.Write((int)line.Parts[j]);
                            }

                            for (int j = 0; j < line.NumPoints; j++)
                            {
                                EVPoint point = line.Points[j] as EVPoint;
                                writer.Write(point.X);
                                writer.Write(point.Y);
                            }
                        }
                        
                        break;
                    case ShapeType.Polygon:
                        for (int i = 0; i < mRecordDic.Count; i++)
                        {
                            EVPolygon gon = mRecordDic[(ulong)(i + 1)].GeoShape as EVPolygon;
                            ShxData shx = new ShxData();
                            shx.ContentLength = gon.DataLength;
                            shx.Offset = offset;
                            offset += shx.ContentLength + 4;
                            shxDic.Add(gon.RecordNum, shx);
                            //写入shp
                            byte[] lbtRecorderNum = BitConverter.GetBytes(gon.RecordNum);
                            byte[] bbtRecorderNum = ByteTransUtil.little2big(lbtRecorderNum);
                            writer.Write(bbtRecorderNum);

                            byte[] lbtDataLength = BitConverter.GetBytes(gon.DataLength);
                            byte[] bbtDataLength = ByteTransUtil.little2big(lbtDataLength);
                            writer.Write(bbtDataLength);

                            writer.Write((int)gon.GeoType);
                            writer.Write(gon.Xmin);
                            writer.Write(gon.Ymin);
                            writer.Write(gon.Xmax);
                            writer.Write(gon.Ymax);

                            writer.Write(gon.NumParts);
                            writer.Write(gon.NumPoints);

                            for (int j = 0; j < gon.NumParts; j++)
                            {
                                writer.Write((int)gon.Parts[j]);
                            }

                            for (int j = 0; j < gon.NumPoints; j++)
                            {
                                EVPoint point = gon.Points[j] as EVPoint;
                                writer.Write(point.X);
                                writer.Write(point.Y);
                            }
                        }
                       
                        break;
                }
               
            }
            return shxDic;
        }
        /// <summary>
        /// 根据RecordDic求header
        /// </summary>
        private void TransHeader() {
            if (mHeader == null) {
                mHeader = new ShpHeader();
            }
    
            mHeader.FileCode = 9994;//固定
            mHeader.FileLength = 50;//文件头
            foreach (var item in mRecordDic) {
                mHeader.FileLength += item.Value.GeoShape.DataLength+4;//每条记录多出4字
            }
            mHeader.Version = 1000;//固定
            mHeader.GeoType = mRecordDic[1].GeoShape.GeoType;
            GetBorder();//求包围盒
        }
        /// <summary>
        /// 求包围盒
        /// </summary>
        private void GetBorder() {
            mHeader.Zmin = 0;
            mHeader.Zmax = 0;
            mHeader.Mmin = 0;
            mHeader.Mmax = 0;

            double Xmin = 0;
            double Ymin = 0;
            double Xmax = 0;
            double Ymax = 0;
            switch (mHeader.GeoType) { 
                case ShapeType.Point:
                    EVPoint pP = mRecordDic[1].GeoShape as EVPoint;
                    Xmin = pP.X;
                    Ymin = pP.Y;
                    Xmax = pP.X;
                    Ymax = pP.Y;
                    for (ulong i = 0; i <(ulong)mRecordDic.Count;i++ )
                    {
                        EVPoint pont = mRecordDic[i+1].GeoShape as EVPoint;
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
                    mHeader.Xmin = Xmin;
                    mHeader.Ymin = Ymin;
                    mHeader.Xmax = Xmax;
                    mHeader.Ymax = Ymax;
                    break;
                case ShapeType.PolyLine:
                    EVPolyLine pL = mRecordDic[1].GeoShape as EVPolyLine;
                    Xmin = pL.Xmin;
                    Ymin = pL.Ymin;
                    Xmax = pL.Xmax;
                    Ymax = pL.Ymax;
                    for (ulong i = 0; i <(ulong)mRecordDic.Count;i++ )
                    {
                        EVPolyLine line = mRecordDic[i + 1].GeoShape as EVPolyLine;
                        if (Xmin > line.Xmin)
                        {
                            Xmin = line.Xmin;
                        }
                        if (Xmax < line.Xmax)
                        {
                            Xmax = line.Xmax;
                        }
                        if (Ymin > line.Ymin)
                        {
                            Ymin = line.Ymin;
                        }
                        if (Ymax < line.Ymax)
                        {
                            Ymax = line.Ymax;
                        }
                    }
                    mHeader.Xmin = Xmin;
                    mHeader.Ymin = Ymin;
                    mHeader.Xmax = Xmax;
                    mHeader.Ymax = Ymax;
                    break;
                case ShapeType.Polygon:
                    EVPolygon pG = mRecordDic[1].GeoShape as EVPolygon;
                    Xmin = pG.Xmin;
                    Ymin = pG.Ymin;
                    Xmax = pG.Xmax;
                    Ymax = pG.Ymax;
                    for (ulong i = 0; i <(ulong)mRecordDic.Count;i++ )
                    {
                        EVPolygon gon = mRecordDic[i + 1].GeoShape as EVPolygon;
                        if (Xmin > gon.Xmin)
                        {
                            Xmin = gon.Xmin;
                        }
                        if (Xmax < gon.Xmax)
                        {
                            Xmax = gon.Xmax;
                        }
                        if (Ymin > gon.Ymin)
                        {
                            Ymin = gon.Ymin;
                        }
                        if (Ymax < gon.Ymax)
                        {
                            Ymax = gon.Ymax;
                        }
                    }
                    mHeader.Xmin = Xmin;
                    mHeader.Ymin = Ymin;
                    mHeader.Xmax = Xmax;
                    mHeader.Ymax = Ymax;
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
