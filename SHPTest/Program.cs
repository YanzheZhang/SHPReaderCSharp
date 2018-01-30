using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIS.HPU.ZYZ.SHP.DBF;
using System.IO;
using GIS.HPU.ZYZ.SHP.SHP;

namespace SHPTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string testPath = AppDomain.CurrentDomain.BaseDirectory;
            //dbf写
            DbfTestWrite(Path.Combine(testPath, "testdata\\test1.dbf"));
            //dbf读
            //DbfTestRead(Path.Combine(testPath, "testdata\\test.dbf"));

            //shp写入
            ShpTestWrite(Path.Combine(testPath, "testdata\\test1.shp"));
            //shp读取
            //ShpTestRead(Path.Combine(testPath, "testdata\\test.shp"));// 结果查看\\实施方案_项目区边界
            //ShpTestRead(Path.Combine(testPath, "testdata\\点.shp"));
            //ShpTestRead(Path.Combine(testPath, "testdata\\线.shp"));
            //ShpTestRead(Path.Combine(testPath, "testdata\\面.shp"));
            Console.ReadKey();
        }

        private static void DbfTestWrite(string filepath)
        {
            var odbf = new DbfFile(Encoding.UTF8);//编码 Encoding.GetEncoding(936)
            odbf.Open(filepath, FileMode.Create);
            //创建列头
            odbf.Header.AddColumn(new DbfColumn("编号", DbfColumn.DbfColumnType.Character, 20, 0));
            odbf.Header.AddColumn(new DbfColumn("名称", DbfColumn.DbfColumnType.Character, 20, 0));
            odbf.Header.AddColumn(new DbfColumn("地址", DbfColumn.DbfColumnType.Character, 20, 0));
            odbf.Header.AddColumn(new DbfColumn("时间", DbfColumn.DbfColumnType.Date));
            odbf.Header.AddColumn(new DbfColumn("余额", DbfColumn.DbfColumnType.Number, 15, 3));

            var orec = new DbfRecord(odbf.Header) { AllowDecimalTruncate = true };
            List<User> list = User.GetList();
            //foreach (var item in list)
            //{
            User item=list[0];
                orec[0] = item.UserCode;
                orec[1] = item.UserName;
                orec[2] = item.Address;
                orec[3] = item.date.ToString("yyyy-MM-dd HH:mm:ss");
                orec[4] = item.money.ToString();
                odbf.Write(orec, true);
            //}
            odbf.Close();
        }

        private static void DbfTestRead(string filepath)
        {
            var odbf = new DbfFile(Encoding.UTF8);//编码
            odbf.Open(filepath,FileMode.Open);
            DbfHeader header= odbf.Header;
            Console.WriteLine(header.ColumnCount);
            List<User> list = new List<User>();
            for (int i = 0; i < header.RecordCount; i++)
            {
                DbfRecord record = odbf.Read(i);
                if (record.ColumnCount >= 5)
                {
                    User item = new User();
                    item.Address = record["地址"].Trim();//record.Column(record.FindColumn("编号"));
                    item.date = record.GetDateValue(record.FindColumn("时间"));
                    item.money = Convert.ToDecimal(record["余额"]);
                    item.UserCode = record["编号"].Trim();
                    item.UserName = record["名称"].Trim();
                    list.Add(item);
                }
            }
            Console.Read();
        }

        private static void ShpTestWrite(string filepath) {
            string wkt = "POLYGON ((125.96146242562871 42.286033095153826,125.96045391503911 42.288221777709978,125.961333679596 42.289101542266863,125.96285717431645 42.289337576660174,125.96596853677373 42.288951338562029,125.96738474313359 42.287942827972429,125.96753494683843 42.286698282989519,125.96611874047856 42.285453738006609,125.96395151559453 42.285861433776873,125.96285717431645 42.286590994628924,125.96225635949712 42.28618329885866,125.962234901825 42.28618329885866,125.96146242562871 42.286033095153826),(125.96146242562871 42.286033095153826,125.96045391503911 42.288221777709978,125.961333679596 42.289101542266863,125.96285717431645 42.289337576660174,125.96596853677373 42.288951338562029,125.96738474313359 42.287942827972429,125.96753494683843 42.286698282989519,125.96611874047856 42.285453738006609,125.96395151559453 42.285861433776873,125.96285717431645 42.286590994628924,125.96225635949712 42.28618329885866,125.962234901825 42.28618329885866,125.96146242562871 42.286033095153826))";
            //string wkt = "POINT (125.96146242562871 42.286033095153826)";
            List<string> lwkt = new List<string>();
            lwkt.Add(wkt);
            var oshp = new ShpFile();
            oshp.Creat(filepath, FileMode.Create);
            oshp.WriteShp(lwkt);
            oshp.WriteShx();
            oshp.Close();
        }

        private static void ShpTestRead(string filepath) 
        {
            var oshp = new ShpFile();
            oshp.Open(filepath, FileMode.Open);
            oshp.ReadShxRecord();
            oshp.ReadShpRecord();
            oshp.ShpFileProject = ShapeProject.WGS_1984_Albers;
            oshp.CoordTransPro2Geo();//投影转换
            string wkt = oshp.ShpRecord.RecordDic[1].WKTStr;//获取wkt
            Console.WriteLine(wkt);
        }

    }
    /// <summary>
    /// 面图斑数据结构
    /// </summary>
    public class MapPolygon {
        public string Code { get; set; }
        public string Mark { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public float Number { get; set; }
        public float Slope { get; set; }
    }
    /// <summary>
    /// 测试数据结构
    /// </summary>
    public class User
    {
        public string UserCode { get; set; }
        public string UserName { get; set; }
        public string Address { get; set; }
        public DateTime date { get; set; }
        public decimal money { get; set; }

        public static List<User> GetList()
        {
            List<User> list = new List<User>();
            list.Add(new User() { UserCode = "A1", UserName = "张三", Address = "上海杨浦", date = DateTime.Now, money = 1000.12m });
            list.Add(new User() { UserCode = "A2", UserName = "李四", Address = "湖北武汉", date = DateTime.Now, money = 31000.008m });
            list.Add(new User() { UserCode = "A3", UserName = "王子龙", Address = "陕西西安", date = DateTime.Now, money = 2000.12m });
            list.Add(new User() { UserCode = "A4", UserName = "李三", Address = "北京", date = DateTime.Now, money = 3000.12m });
            return list;
        }
    }

}
