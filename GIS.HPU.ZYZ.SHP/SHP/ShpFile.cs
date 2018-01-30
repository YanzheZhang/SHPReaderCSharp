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
using GIS.HPU.ZYZ.SHP.SHX;

namespace GIS.HPU.ZYZ.SHP.SHP
{
    /// <summary>
    /// 负责shp文件的读写
    /// 文件的读取和写入同一实例对象不能同时使用！
    /// 可以从shp文件和数据库的数据类中读取Header和Record信息
    /// 如果是写入信息，则需要先从数据库的数据类中读取Record信息，然后计算出Header信息
    /// </summary>
    /// <remarks>
    /// 读取shp文件流程：
    ///  var oshp = new ShpFile();
    ///      oshp.Open(filepath, FileMode.Open);//open的同时执行了头文件的读取
    ///      oshp.ReadShxRecord();//读取索引文件 此处需要注意shp头文件和shx头文件的FileLength不同其他都一样，这个在读取文件没影响，shx头文件直接使用shp的头文件
    ///      oshp.ReadShpRecord();//读取记录
    ///      oshp.ShpFileProject = ShapeProject.WGS_1984_Albers;//设置坐标系
    ///      oshp.CoordTransPro2Geo();//投影转换
    ///      string wkt = oshp.ShpRecord.RecordDic[1].WKTStr;//获取每个记录的wkt
    /// </remarks>
    public class ShpFile
    {
        /// <summary>
        /// 读写shp文件头
        /// 读写操作放在ShpHeader
        /// </summary>
        protected ShpHeader mHeader;
        /// <summary>
        /// 读写shp记录
        /// 读写操作放在ShpRecord
        /// </summary>
        protected ShpRecord mShpRecord;
        /// <summary>
        /// 读写shx
        /// 读写操作放在ShxRecord
        /// </summary>
        protected ShxRecord mShxRecord;
        /// <summary>
        /// 当前数据坐标系
        /// </summary>
        private ShapeProject mShpFileProject;
        /// <summary>
        /// 标记是否已经读取文件头
        /// </summary>
        protected bool mHeaderWritten = false;
        /// <summary>
        /// 标记是否已经设置wkt
        /// </summary>
        protected bool mFileCreat = false;
        /// <summary>
        /// 读写shp文件流.
        /// </summary>
        protected Stream mShpFile = null;
        /// <summary>
        /// 读写shx文件流.
        /// </summary>
        protected Stream mShxFile = null;
        /// <summary>
        /// shp文件读取流
        /// </summary>
        protected BinaryReader mShpFileReader = null;
        /// <summary>
        /// shx文件读取流
        /// </summary>
        protected BinaryReader mShxFileReader = null;
        /// <summary>
        /// shp文件写入流
        /// </summary>
        protected BinaryWriter mShpFileWriter = null;
        /// <summary>
        /// shx文件写入流
        /// </summary>
        protected BinaryWriter mShxFileWriter = null;
        /// <summary>
        /// 当前打开文件的文件名.
        /// </summary>
        protected string mFileName = "";
        /// <summary>
        /// 记录读取的记录个数
        /// </summary>
        protected int mRecordsReadCount = 0;
        /// <summary>
        /// keep these values handy so we don't call functions on every read.
        /// </summary>
        protected bool mIsForwardOnly = false;
        protected bool mIsReadOnly = false;

        #region 属性
        /// <summary>
        /// 返回文件流的文件名 
        /// </summary>
        public string FileName
        {
            get
            {
                return mFileName;
            }
        }
        /// <summary>
        /// 如果不能写入shp文件流返回true
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return mIsReadOnly;
            }
        }
        /// <summary>
        /// 如果可以指针可以移动到文件流的任意位置返回true 
        /// </summary>
        public bool IsForwardOnly
        {
            get
            {
                return mIsForwardOnly;
            }
        }
        /// <summary>
        /// 当前数据坐标系
        /// </summary>
        public ShapeProject ShpFileProject
        {
            get { return mShpFileProject; }
            set { mShpFileProject = value; }
        }
        /// <summary>
        /// 头文件信息操作类 
        /// </summary>
        public ShpHeader Header
        {
            get
            {
                return mHeader;
            }
        }
        /// <summary>
        /// shp文件记录信息操作类
        /// </summary>
        public ShpRecord ShpRecord
        {
            get 
            {
                return mShpRecord;
            }
        }
        /// <summary>
        /// shx文件记录信息操作类
        /// </summary>
        public ShxRecord ShxRecord
        {
            get
            {
                return mShxRecord;
            }
        }
        #endregion 

        /// <summary>
        /// 
        /// </summary>
        public ShpFile()
        {
            mHeader = new ShpHeader();
        }

        /// <summary>
        /// 打开shp、shx文件流 
        /// 读取shp、shx信息时需要首先执行此操作
        /// 同时操作头文件读取和ShxRecord、ShpRecord示例化
        /// </summary>
        /// <param name="ofs"></param>
        public void Open(Stream shpStream,Stream shxStream)
        {
            if (mShpFile != null)
                Close();

            mShpFile = shpStream;
            mShxFile = shxStream;
            mShpFileReader = null;
            mShpFileWriter = null;
            mShxFileReader = null;
            mShxFileWriter = null;
            if (mShpFile.CanRead)
                mShpFileReader = new BinaryReader(mShpFile);
            if (mShpFile.CanWrite)
                mShpFileWriter = new BinaryWriter(mShpFile);

            if (mShxFile.CanRead)
                mShxFileReader = new BinaryReader(mShxFile);
            if (mShxFile.CanWrite)
                mShxFileWriter = new BinaryWriter(mShxFile);
            //reset position
            mRecordsReadCount = 0;
            //assume header is not written
            mHeaderWritten = false;

            //read the header
            if (shpStream.CanRead)
            {
                //try to read the header...
                try
                {
                    mHeader.Read(mShpFileReader);
                    mHeaderWritten = true;

                }
                catch (EndOfStreamException)
                {
                    //could not read header, file is empty
                    mHeader = new ShpHeader();
                    mHeaderWritten = false;
                }
            }
            if (mShpFile != null)
            {
                mIsReadOnly = !mShpFile.CanWrite;
                mIsForwardOnly = !mShpFile.CanSeek;
            }
            mShxRecord = new ShxRecord(mHeader);
            mShpRecord = new ShpRecord(mHeader);
        }
        /// <summary>
        /// 打开shp、shx文件流 
        /// 读取shp、shx信息时需要首先执行此操作
        /// </summary>
        /// <param name="sPath">文件全路径</param>
        /// <param name="mode"></param>
        public void Open(string sPath, FileMode mode)
        {
            mFileName = sPath;
            string shxpath = mFileName.Remove(mFileName.Length - 1, 1) + "x";
            Open(File.Open(sPath, mode), File.Open(shxpath, mode));
        }

        /// <summary>
        /// 读取shx文件的记录信息
        /// 先执行ReadShxRecord 再执行ReadShpRecord
        /// 先执行Open此函数才有效
        /// </summary>
        public void ReadShxRecord() 
        {
            if (mHeaderWritten && mShxFile.CanRead)
            {
                mShxRecord.Read(mShxFileReader);
            }
        }

        /// <summary>
        /// 读取shp文件的记录信息
        /// 先执行ReadShxRecord 再执行ReadShpRecord
        /// 先执行Open此函数才有效
        /// </summary>
        public void ReadShpRecord() 
        {
            if (mHeaderWritten && mShpFile.CanRead) {
                mShpRecord.Read(mShpFileReader);
            }
        }
        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="shpStream"></param>
        /// <param name="shxStream"></param>
        public void Creat(Stream shpStream, Stream shxStream) 
        {
            if (mShpFile != null)
                Close();

            mShpFile = shpStream;
            mShxFile = shxStream;
            mShpFileWriter = null;
            mShxFileWriter = null;
            //if (mShpFile.CanRead)
            //    mShpFileReader = new BinaryReader(mShpFile);
            if (mShpFile.CanWrite)
                mShpFileWriter = new BinaryWriter(mShpFile);

            //if (mShxFile.CanRead)
            //    mShxFileReader = new BinaryReader(mShxFile);
            if (mShxFile.CanWrite)
                mShxFileWriter = new BinaryWriter(mShxFile);
            //reset position
            //mRecordsReadCount = 0;
            //assume header is not written
            mFileCreat = true;

            if (mShpFile != null)
            {
                mIsReadOnly = !mShpFile.CanWrite;
                mIsForwardOnly = !mShpFile.CanSeek;
            }
            //mShxRecord = new ShxRecord(mHeader);
            mShpRecord = new ShpRecord(mHeader);
        }
        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="sPath"></param>
        /// <param name="mode"></param>
        public void Creat(string sPath, FileMode mode)
        {
            mFileName = sPath;
            string shxpath = mFileName.Remove(mFileName.Length - 1, 1) + "x";
            Creat(File.Open(sPath, mode), File.Open(shxpath, mode));
        }
        /// <summary>
        /// 写入shx
        /// 此时需要注意头文件不能和shp头文件关联，FileLength不同
        /// </summary>
        public void WriteShx() 
        {
            if (mFileCreat) {
                mShxRecord.Write(mShxFileWriter);
            }
        }
        /// <summary>
        /// 写入shp
        /// </summary>
        /// <param name="wktList"></param>
        public void WriteShp(List<string> wktList) 
        {
            if (mFileCreat) {
                mShpRecord.GetWKTInfo(wktList);
                mShxRecord = new ShxRecord(mShpRecord.Header);
                mHeader.Write(mShpFileWriter);
                mShxRecord.RecordDic= mShpRecord.Write(mShpFileWriter);
            }
        }
        /// <summary>
        /// 将WGS_1984_Albers坐标系转换为GCS_WGS_1984
        /// </summary>
        public void CoordTransPro2Geo() 
        {
            if (mShpFileProject == ShapeProject.GCS_WGS_1984){}
            else {
                mShpRecord.CoordTransPro2Geo();
                mShpFileProject = ShapeProject.GCS_WGS_1984;
            }
        }
        /// <summary>
        /// 将GCS_WGS_1984坐标系转换为WGS_1984_Albers
        /// </summary>
        public void CoordTransGeo2Pro()
        {
            if (mShpFileProject == ShapeProject.WGS_1984_Albers) { }
            else
            {
                mShpRecord.CoordTransGeo2Pro();
                mShpFileProject = ShapeProject.WGS_1984_Albers;
            }
        }
        /// <summary>
        /// 更新header信息 清除(flush)buffers 关闭streams
        /// 在操作完shp文件后必须调用此方法 
        /// </summary>
        public void Close()
        {
            //Empty header...
            //--------------------------------
            mHeader = new ShpHeader();
            mHeaderWritten = false;

            //reset current record index
            //--------------------------------
            mRecordsReadCount = 0;

            //Close streams...
            //--------------------------------
            if (mShpFileWriter != null)
            {
                mShpFileWriter.Flush();
                mShpFileWriter.Close();
            }
            if (mShpFileReader != null)
                mShpFileReader.Close();
            if (mShpFile != null)
                mShpFile.Close();

            if (mShxFileWriter != null)
            {
                mShxFileWriter.Flush();
                mShxFileWriter.Close();
            }
            if (mShxFileReader != null)
                mShxFileReader.Close();
            if (mShxFile != null)
                mShxFile.Close();
            //set streams to null
            //--------------------------------
            mShpFileReader = null;
            mShpFileWriter = null;
            mShpFile = null;

            mShxFileReader = null;
            mShxFileWriter = null;
            mShxFile = null;

            mFileName = "";
        }
   
    }
}
