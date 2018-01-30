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
using GIS.HPU.ZYZ.SHP.Util;

namespace GIS.HPU.ZYZ.SHP.SHP
{
    /// <summary>
    /// 坐标文件(.shp)用于记录空间坐标信息。
    /// 它由头文件和实体信息两部分构成。
    /// 坐标文件的文件头是一个长度固定(100 bytes)的记录段，一共有9个int型和7个double型数据。
    /// </summary>
    /// <remarks>
    /// Position 	Field 			Value 			Type 		Order      
    /// Byte 0		File Code 		9994 			Integer 	Big       |-----
    /// Byte 4		Unused 			0 			    Integer 	Big       |Value 		Shape Type
    /// Byte 8 		Unused 			0 			    Integer 	Big       |0      		Null Shape
    /// Byte 12 	Unused 			0 			    Integer 	Big       |1      		Point
    /// Byte 16 	Unused 			0 			    Integer 	Big       |3 			PolyLine
    /// Byte 20 	Unused 			0 			    Integer 	Big       |5 			Polygon
    /// Byte 24 	File Length 	File Length 	Integer 	Big       |8 			MultiPoint
    /// Byte 28 	Version 		1000 			Integer	 	Little    |11 			PointZ
    /// Byte 32 	Shape Type 		Shape Type		Integer 	Little----|13 			PolyLineZ
    /// Byte 36 	Bounding Box 	Xmin 			Double 		Little    |15 			PolygonZ
    /// Byte 44 	Bounding Box 	Ymin 			Double 		Little    |18 			MultiPointZ
    /// Byte 52 	Bounding Box 	Xmax 			Double 		Little    |21 			PointM
    /// Byte 60 	Bounding Box 	Ymax 			Double 		Little    |23 			PolyLineM
    /// Byte 68* 	Bounding Box 	Zmin 			Double 		Little    |25 			PolygonM
    /// Byte 76* 	Bounding Box 	Zmax	 		Double 		Little    |28 			MultiPointM
    /// Byte 84* 	Bounding Box 	Mmin 			Double 		Little    |31 			MultiPatch  
    /// Byte 92* 	Bounding Box	Mmax 			Double 		Little    |----
    ///                                                                    
    /// </remarks>                                                         
    public class ShpHeader                                                 
    {                                                                      
        private int mFileCode;                                             
        private int mFileLength;
        private int mVersion;
        private ShapeType mGeoType;
        private double mXmin;
        private double mYmin;
        private double mXmax;
        private double mYmax;
        private double mZmin;
        private double mZmax;
        private double mMmin;
        private double mMmax;
        #region 属性
        /// <summary>
        /// 文件code
        /// 9994 big位序
        /// </summary>
        public int FileCode
        {
            get { return mFileCode; }
            set { mFileCode = value; }
        }
        /// <summary>
        /// 文件长度
        /// 文件的实际长度 
        /// big位序
        /// </summary>
        public int FileLength
        {
            get { return mFileLength; }
            set { mFileLength = value; }
        }
        /// <summary>
        /// 版本号 
        /// 1000 
        /// Little位序
        /// </summary>
        public int Version
        {
            get { return mVersion; }
            set { mVersion = value; }
        }
        /// <summary>
        /// 几何类型
        /// 表示这个Shapefile文件所记录的空间数据的几何类型
        /// Little位序
        /// </summary>
        public ShapeType GeoType
        {
            get { return mGeoType; }
            set { mGeoType = value; }
        }
        /// <summary>
        /// 空间数据所占空间范围的X方向最小值
        /// </summary>
        public double Xmin
        {
            get { return mXmin; }
            set { mXmin = value; }
        }
        /// <summary>
        /// 空间数据所占空间范围的Y方向最小值
        /// </summary>
        public double Ymin
        {
            get { return mYmin; }
            set { mYmin = value; }
        }
        /// <summary>
        /// 空间数据所占空间范围的X方向最大值
        /// </summary>
        public double Xmax
        {
            get { return mXmax; }
            set { mXmax = value; }
        }
        /// <summary>
        /// 空间数据所占空间范围的Y方向最大值
        /// </summary>
        public double Ymax
        {
            get { return mYmax; }
            set { mYmax = value; }
        }
        /// <summary>
        /// 空间数据所占空间范围的Z方向最小值
        /// </summary>
        public double Zmin
        {
            get { return mZmin; }
            set { mZmin = value; }
        }
        /// <summary>
        /// 空间数据所占空间范围的Z方向最大值
        /// </summary>
        public double Zmax
        {
            get { return mZmax; }
            set { mZmax = value; }
        }
        /// <summary>
        /// 最小Measure值
        /// </summary>
        public double Mmin
        {
            get { return mMmin; }
            set { mMmin = value; }
        }
        /// <summary>
        /// 最大Measure值
        /// </summary>
        public double Mmax
        {
            get { return mMmax; }
            set { mMmax = value; }
        }
        #endregion

        /// <summary>
        /// 读取header数据
        /// 确定读取时文件流位置在开始处
        /// 当读取完成文件流位置在第一个记录处
        /// </summary>
        /// <param name="reader"></param>
        public void Read(BinaryReader reader)
        {
            // type of reader.

            mFileCode = reader.ReadInt32();
            mFileCode = (int)ByteTransUtil.big2little(mFileCode);
            reader.ReadBytes(20);//预留5个inter
            mFileLength = reader.ReadInt32();
            mFileLength = (int)ByteTransUtil.big2little(mFileLength);//以字为单位，一字等于2字节，等于16位

            mVersion = reader.ReadInt32();
            int typeTemp = reader.ReadInt32();
            mGeoType = (ShapeType)typeTemp;//目前考虑点 线 面三种
            mXmin = reader.ReadDouble();
            mYmin = reader.ReadDouble();
            mXmax = reader.ReadDouble();
            mYmax = reader.ReadDouble();
            mZmin = reader.ReadDouble();
            mZmax = reader.ReadDouble();
            mMmin = reader.ReadDouble();
            mMmax = reader.ReadDouble();
        }

        /// <summary>
        /// Encoding must be ASCII for this binary writer.
        /// </summary>
        /// <param name="writer"></param>
        /// <remarks>
        /// 参考shp文件头文件结构.
        /// </remarks>
        public void Write(BinaryWriter writer)
        {
            writer.Flush();//清除
            byte[] lbtFileCode = BitConverter.GetBytes(mFileCode);  //将int转变为byte
            byte[] bbtFileCode = ByteTransUtil.little2big(lbtFileCode);
            writer.Write(bbtFileCode);
            int Unused = 0; //未使用 一共5个 big
            byte[] lbtUnused = BitConverter.GetBytes(Unused);  //将int转变为byte
            byte[] bbtUnused = ByteTransUtil.little2big(lbtUnused);
            for (int i = 0; i < 5; i++)
            {
                writer.Write(bbtUnused);
            }
            byte[] lbtFileLength = BitConverter.GetBytes(mFileLength);  //将int转变为byte
            byte[] bbtFileLength = ByteTransUtil.little2big(lbtFileLength);
            writer.Write(bbtFileLength);
            writer.Write(mVersion);
            int tyeptemp = (int)mGeoType;
            writer.Write(tyeptemp);//---------------------
            writer.Write(mXmin);
            writer.Write(mYmin);
            writer.Write(mXmax);
            writer.Write(mYmax);
            writer.Write(mZmin);
            writer.Write(mZmax);
            writer.Write(mMmin);
            writer.Write(mMmax);
        }
       
    }
}
