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

namespace GIS.HPU.ZYZ.SHP.SHX
{
    /// <summary>
    /// shx文件记录内容的数据结构
    /// </summary>
    /// <remarks>
    /// 每一条记录包括偏移量（offset）和记录段长度（Content Length）两个记录项，它们的位序都是big，两个记录项都是int型。
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
    public class ShxData
    {
        /// <summary>
        /// 位移量（Offset）
        /// 表示坐标文件中的对应记录的起始位置相对于坐标文件起始位置的位移量。
        /// 以字为单位，一字等于2字节，等于16位   
        /// 长度包括每条记录的RecordNum和DataLength共8字节4位
        /// </summary>
        public int Offset;
        /// <summary>
        /// 记录长度（Content Length） 
        /// 表示坐标文件中的对应记录的长度
        /// 以字为单位，一字等于2字节，等于16位  
        /// 长度不包括每条记录的RecordNum和DataLength共8字节4位。
        /// </summary>
        public int ContentLength;
    }
}
