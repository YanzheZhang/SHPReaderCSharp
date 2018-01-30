///
/// Author: YanzheZhang
/// Date: 26/1/2018
/// Desc:Add type 'F' for DbfColumnType. After ARCGIS10.0 the dbf file add.
/// 
/// Revision History:
/// -----------------------------------
///   Author:Ahmed Lacevic
///   Date:12/1/2007
///   Desc:
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace GIS.HPU.ZYZ.SHP.DBF
{
    public class DbfDataTruncateException : Exception
    {

        public DbfDataTruncateException(string smessage)
            : base(smessage)
        {
        }

        public DbfDataTruncateException(string smessage, Exception innerException)
            : base(smessage, innerException)
        {
        }

        public DbfDataTruncateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

    }
}
