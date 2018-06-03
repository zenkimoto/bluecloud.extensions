using System;
using BlueCloud.Extensions.Data;

namespace BlueCloud.Extensions.Tests.Model
{
    public class Employee
    {
        [DbField("EmployeeId")]
        public long EmployeeId { get; set; }

        [DbField("LastName")]
        public string LastName { get; set; }

        [DbField("FirstName")]
        public string FirstName { get; set; }

        [DbField("Title")]
        public string Title { get; set; }

        [DbField("ReportsTo")]
        public long? ReportsTo { get; set; }

        [DbField("BirthDate")]
        public DateTime BirthDate { get; set; }

        [DbField("HireDate")]
        public DateTime HireDate { get; set; }
    }
}
