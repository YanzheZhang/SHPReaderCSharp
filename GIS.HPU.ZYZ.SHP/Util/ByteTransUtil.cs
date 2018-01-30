using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GIS.HPU.ZYZ.SHP.Util
{
    /// <summary>
    /// 位操作工具类
    /// 提供big little转换
    /// </summary>
    public static class ByteTransUtil
    {
        /// <summary>
        /// 将big位序转换为little位序
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static ulong big2little(int code)
        {
            ulong value;
            value = ((((ulong)(code) & (ulong)0x000000ffUL) << 24) | (((ulong)(code) & (ulong)0x0000ff00UL) << 8) | (((ulong)(code) & (ulong)0x00ff0000UL) >> 8) | (((ulong)(code) & (ulong)0xff000000UL) >> 24));
            return value;
        }
        /// <summary>
        /// 将little位序转换为big位序
        /// </summary>
        /// <param name="lbt"></param>
        /// <returns></returns>
        public static byte[] little2big(byte[] lbt)
        {
            byte[] bbt = new byte[4];
            bbt[0] = lbt[3];
            bbt[1] = lbt[2];
            bbt[2] = lbt[1];
            bbt[3] = lbt[0];
            return bbt;
        }
    }
}
