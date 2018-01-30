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
using GIS.HPU.ZYZ.SHP.SHP;
using System.IO;
using GIS.HPU.ZYZ.SHP.Util;

namespace GIS.HPU.ZYZ.SHP.SHX
{
    /// <summary>
    /// shx文件详细信息
    /// 包括头文件和记录内容
    /// </summary>
    /// <remarks>
    /// 索引文件（.shx）主要包含坐标文件的索引信息
    /// 文件中每个记录包含对应的坐标文件记录距离坐标文件的文件头的偏移量。
    /// 通过索引文件可以很方便地在坐标文件中定位到指定目标的坐标信息。
    /// 其中文件头部分是一个长度固定(100 bytes)的记录段，其内容与坐标文件的文件头基本一致。
    /// 它的实体信息以记录为基本单位，每一条记录包括偏移量（offset）和记录段长度（Content Length）两个记录项，它们的位序都是big，两个记录项都是int型。
    /// 
    /// Position 		Field	 		Value 			Type 		Byte Order
    /// Byte 0 			Offset 			Offset 			Integer 	Big
    /// Byte 4 			Content Length 	Content Length 	Integer 	Big
    /// 
    /// 位移量（Offset）表示坐标文件中的对应记录的起始位置相对于坐标文件起始位置的位移量。
    /// 以字为单位，一字等于2字节，等于16位   长度包括每条记录的RecordNum和DataLength共8字节4位
    /// 记录长度（Content Length） 表示坐标文件中的对应记录的长度。 
    /// 以字为单位，一字等于2字节，等于16位  长度不包括每条记录的RecordNum和DataLength共8字节4位
    /// </remarks>
    public class ShxRecord
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
        private Dictionary<ulong, ShxData> mRecordDic = null;
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
        /// value：该条记录的具体内容：ShxData类型
        /// </summary>
        public Dictionary<ulong, ShxData> RecordDic
        {
            get { return mRecordDic; }
            //set { mRecordDic = value; }
        }
        #endregion

         /// <summary>
        /// 构造时传入已经实例化的ShpHeader对象
        /// </summary>
        /// <param name="oHeader"></param>
        public ShxRecord(ShpHeader oHeader) 
        {
            mHeader = oHeader;
        }
        /// <summary>
        /// 读取record数据
        /// 当读取时文件流位置在第一个记录处
        /// </summary>
        /// <param name="reader">shx文件流</param>
        public void Read(BinaryReader reader)
        {
            //如果header为空先读取header shp和shx的文件头一样
            if (mHeader == null)
            {
                mHeader.Read(reader);
            } 
            if (mRecordDic != null)
            {
                mRecordDic.Clear();
            }
            else
            {
                mRecordDic = new Dictionary<ulong, ShxData>();
            }
            if (reader.BaseStream.Position == 0) {
                reader.BaseStream.Seek(100,SeekOrigin.Begin);//跳过文件头
            } 
            while (reader.PeekChar() != -1)
            {
                ShxData item = new ShxData();
                int Offset = reader.ReadInt32();
                int ContentLength = reader.ReadInt32();
                ulong uOffset = ByteTransUtil.big2little(Offset);// 
                ulong uContentLength = ByteTransUtil.big2little(ContentLength);// 
                item.Offset = (int)uOffset;//
                item.ContentLength = (int)uContentLength;
                mRecordDic.Add((ulong)(mRecordDic.Count+1), item);//key值从1开始
            }
        }
    }
}
