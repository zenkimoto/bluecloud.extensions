using System;
using BlueCloud.Extensions.Data;

namespace BlueCloud.Extensions.Tests.Model
{
    public class NumberTest
    {
        [DbField("ID")]
        public long Id { get; set; }

        [DbField("VALUE")]
        public long Value { get; set; }
    }
}
