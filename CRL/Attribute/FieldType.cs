/**
* CRL 快速开发框架 V4.0
* Copyright (c) 2016 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL3
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Attribute
{
    /// <summary>
    /// 字段类型
    /// </summary>
    public enum FieldType
    {
        NONE,
        数据库字段,
        虚拟字段,
        关联字段,
        关联对象
    }
}
