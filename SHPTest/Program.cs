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
            //DbfTestWrite(Path.Combine(testPath, "testdata\\导出t.dbf"));
            //dbf读
            DbfTestRead(Path.Combine(testPath, "testdata\\国家导出\\面.dbf"));// result\\936\\图斑_面 result\\936\\导出t  国家导出\\面  国家标准\\面状措施

            //shp写入
            //ShpTestWrite(Path.Combine(testPath, "testdata\\导出t.shp"));
            //shp读取
            //ShpTestRead(Path.Combine(testPath, "testdata\\91导出\\面.shp"));//  
            //ShpTestRead(Path.Combine(testPath, "testdata\\点.shp"));
            //ShpTestRead(Path.Combine(testPath, "testdata\\线.shp"));
            //ShpTestRead(Path.Combine(testPath, "testdata\\面.shp"));
            Console.ReadKey();
        }

        private static void DbfTestWrite(string filepath)
        {
            //编码Encoding.UTF8 中文字符占三字节 Encoding.GetEncoding(936) Encoding.Default中文字符占二字节
            var odbf = new DbfFile(Encoding.GetEncoding(936));
            odbf.Open(filepath, FileMode.Create);//FileMode.Create OpenOrCreate
            //创建列头
            //odbf.Header.AddColumn(new DbfColumn("编号", DbfColumn.DbfColumnType.Character, 20, 0));
            //odbf.Header.AddColumn(new DbfColumn("名称", DbfColumn.DbfColumnType.Character, 20, 0));
            //odbf.Header.AddColumn(new DbfColumn("地址", DbfColumn.DbfColumnType.Character, 20, 0));
            //odbf.Header.AddColumn(new DbfColumn("时间", DbfColumn.DbfColumnType.Date));
            //odbf.Header.AddColumn(new DbfColumn("余额", DbfColumn.DbfColumnType.Number, 15, 3));

            //var orec = new DbfRecord(odbf.Header) { AllowDecimalTruncate = true };
            //List<User> list = User.GetList();
            ////foreach (var item in list)
            ////{
            //User item=list[0];
            //    orec[0] = item.UserCode;
            //    orec[1] = item.UserName;
            //    orec[2] = item.Address;
            //    orec[3] = item.date.ToString("yyyy-MM-dd HH:mm:ss");
            //    orec[4] = item.money.ToString();
            //    odbf.Write(orec, true);
            ////}

            //写入边界
            //odbf.Header.AddColumn(new DbfColumn("id", DbfColumn.DbfColumnType.Number, 19, 0));
            //var orec = new DbfRecord(odbf.Header) { AllowDecimalTruncate = true };
            //orec[0] = 1.ToString();
            //odbf.Write(orec, true);

            //写入图斑
            odbf.Header.AddColumn(new DbfColumn("图斑编码", DbfColumn.DbfColumnType.Character, 80, 0));
            odbf.Header.AddColumn(new DbfColumn("措施代码", DbfColumn.DbfColumnType.Character, 80, 0));
            odbf.Header.AddColumn(new DbfColumn("措施名称", DbfColumn.DbfColumnType.Character, 80, 0));
            odbf.Header.AddColumn(new DbfColumn("利用现状", DbfColumn.DbfColumnType.Character, 80, 0));
            odbf.Header.AddColumn(new DbfColumn("措施数量", DbfColumn.DbfColumnType.Number, 18, 15));
            odbf.Header.AddColumn(new DbfColumn("坡度", DbfColumn.DbfColumnType.Number, 18, 15));

            var orec = new DbfRecord(odbf.Header) { AllowDecimalTruncate = true };
            List<MapPolygon> list = MapPolygon.GetList();
            foreach (var item in list)
            {
            //MapPolygon item = list[0];
                orec[0] = item.Code;//顺序要与header顺序保持一致
                orec[1] = item.Mark;
                orec[2] = item.Name;
                orec[3] = item.State;
                orec[4] = item.Number.ToString();
                orec[5] = item.Slope.ToString();
                odbf.Write(orec, true);
            }
            odbf.Close();
        }

        private static void DbfTestRead(string filepath)
        {
            //编码Encoding.UTF8 中文字符占三字节 Encoding.GetEncoding(936)中文字符占二字节
            var odbf = new DbfFile(Encoding.GetEncoding(936));//编码
            odbf.Open(filepath,FileMode.Open);
            DbfHeader header= odbf.Header;
            Console.WriteLine(header.ColumnCount);
            //List<User> list = new List<User>();
            //for (int i = 0; i < header.RecordCount; i++)
            //{
            //    DbfRecord record = odbf.Read(i);
            //    if (record.ColumnCount >= 5)
            //    {
            //        User item = new User();
            //        item.Address = record["地址"].Trim();//record.Column(record.FindColumn("编号"));
            //        item.date = record.GetDateValue(record.FindColumn("时间"));
            //        item.money = Convert.ToDecimal(record["余额"]);
            //        item.UserCode = record["编号"].Trim();
            //        item.UserName = record["名称"].Trim();
            //        list.Add(item);
            //    }
            //}

            List<MapPolygon> list = new List<MapPolygon>();
            for (int i = 0; i < header.RecordCount; i++)
            {
                DbfRecord record = odbf.Read(i);
                if (record.ColumnCount >= 5)
                {
                    MapPolygon item = new MapPolygon();
                    item.Code = record[record.FindColumEx("图斑编码")].Trim();// record["图斑编码"].Trim();//record.Column(record.FindColumn("编号"));
                    item.Mark = record[record.FindColumEx("措施代码")].Trim();//record["措施代码"].Trim();
                    item.Name = record[record.FindColumEx("措施名称")].Trim();//record["措施名称"].Trim();
                    item.State = record[record.FindColumEx("利用现状")].Trim();//record["利用现状"].Trim();
                    //item.Number =Convert.ToSingle( record["措施数量"]);
                    //item.Slope = Convert.ToSingle(record["坡度"]);
                    list.Add(item);
                }
            }
            Console.Read();
        }

        private static void ShpTestWrite(string filepath) {
            //string wkt = "POLYGON ((125.96146242562871 42.286033095153826,125.96045391503911 42.288221777709978,125.961333679596 42.289101542266863,125.96285717431645 42.289337576660174,125.96596853677373 42.288951338562029,125.96738474313359 42.287942827972429,125.96753494683843 42.286698282989519,125.96611874047856 42.285453738006609,125.96395151559453 42.285861433776873,125.96285717431645 42.286590994628924,125.96225635949712 42.28618329885866,125.962234901825 42.28618329885866,125.96146242562871 42.286033095153826),(125.96146242562871 42.286033095153826,125.96045391503911 42.288221777709978,125.961333679596 42.289101542266863,125.96285717431645 42.289337576660174,125.96596853677373 42.288951338562029,125.96738474313359 42.287942827972429,125.96753494683843 42.286698282989519,125.96611874047856 42.285453738006609,125.96395151559453 42.285861433776873,125.96285717431645 42.286590994628924,125.96225635949712 42.28618329885866,125.962234901825 42.28618329885866,125.96146242562871 42.286033095153826))";
            //string wkt = "LINESTRING ((125.96146242562871 42.286033095153826,125.96045391503911 42.288221777709978,125.961333679596 42.289101542266863,125.96285717431645 42.289337576660174,125.96596853677373 42.288951338562029,125.96738474313359 42.287942827972429,125.96753494683843 42.286698282989519,125.96611874047856 42.285453738006609,125.96395151559453 42.285861433776873,125.96285717431645 42.286590994628924,125.96225635949712 42.28618329885866,125.962234901825 42.28618329885866,125.96146242562871 42.286033095153826),(125.96146242562871 42.286033095153826,125.96045391503911 42.288221777709978,125.961333679596 42.289101542266863,125.96285717431645 42.289337576660174,125.96596853677373 42.288951338562029,125.96738474313359 42.287942827972429,125.96753494683843 42.286698282989519,125.96611874047856 42.285453738006609,125.96395151559453 42.285861433776873,125.96285717431645 42.286590994628924,125.96225635949712 42.28618329885866,125.962234901825 42.28618329885866,125.96146242562871 42.286033095153826))";
            //string wkt = "POINT (125.96146242562871 42.286033095153826)";
            //边界
            //string border = "POLYGON ((124.797090749881 43.4559641037949,124.833482961795 43.4679804001816,124.848589162967 43.4528741990098,124.855455618045 43.4346780930527,124.85854552283 43.4171686326035,124.861978750369 43.3986292038926,124.851679067752 43.3872995530137,124.840349416873 43.3728799973496,124.82421324744 43.3701334153184,124.804987173221 43.3704767380723,124.784387807987 43.3708200608262,124.777178030155 43.378716484166,124.773744802615 43.3996591721543,124.771684866092 43.4236917649277,124.797090749881 43.4559641037949))";
            //图斑
            string p1 = "POLYGON ((124.799150686405 43.4515009079941,124.808077078006 43.4490976487168,124.807390432498 43.4415445481309,124.801210622928 43.4401712571152,124.795374136112 43.4449777756699,124.799150686405 43.4515009079941))";
            string p2 = "POLYGON ((124.792970876834 43.4370813523301,124.797777395389 43.4288416062363,124.801553945682 43.4168253098496,124.795717458865 43.4151086960801,124.787821035526 43.422661796666,124.792284231326 43.4284982834824,124.792970876834 43.4370813523301))";
            string p3 = "POLYGON ((124.82249663367 43.441201225377,124.830736379764 43.4532175217637,124.835199575565 43.4442911301621,124.83657286658 43.4339914475449,124.832452993533 43.4309015427598,124.826273183963 43.4315881882676,124.82249663367 43.441201225377))";
            string p4 = "POLYGON ((124.807733755252 43.4130487595566,124.8163168241 43.4212885056504,124.821466665408 43.4096155320176,124.821466665408 43.4024057541855,124.815630178592 43.397255912877,124.80842040076 43.400689140416,124.807733755252 43.4130487595566))";

            List<string> lwkt = new List<string>();
            //lwkt.Add(border);
            lwkt.Add(p1);
            lwkt.Add(p2);
            lwkt.Add(p3);
            lwkt.Add(p4);
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
            //根据实际情况进行投影
            //oshp.ShpFileProject = ShapeProject.WGS_1984_Albers;
            //oshp.CoordTransPro2Geo();//投影转换
            foreach (var item in oshp.ShpRecord.RecordDic)
            {
                string wkt = item.Value.WKTStr;//获取wkt
                Console.WriteLine(wkt);
            }
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

        public static List<MapPolygon> GetList()
        {
            List<MapPolygon> list = new List<MapPolygon>();
            list.Add(new MapPolygon() { Code = "01", Mark = "sbl3", Name = "水保林", State = "耕地", Number = 1.1f, Slope=12f });
            list.Add(new MapPolygon() { Code = "02", Mark = "sbl3", Name = "水保林", State = "耕地", Number = 1.6f, Slope = 2f });
            list.Add(new MapPolygon() { Code = "03", Mark = "sbl3", Name = "水保林", State = "耕地", Number = 5.1f, Slope = 7f });
            list.Add(new MapPolygon() { Code = "04", Mark = "sbl3", Name = "水保林", State = "耕地", Number = 4.1f, Slope = 5f });
            return list;
        }
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
