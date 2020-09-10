using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFramework.BulkOperations.Tests
{
    [Table("EmployeeWithCompressedData")]
    public class EmployeeWithCompressedDataEntity
    {
        [Key]
        public string EmployeeId { get; set; }
        public string First { get; set; }
        public string Last { get; set; }
        public string Team { get; set; }
        public string Organization { get; set; }
        public DateTime HiredDate { get; set; }
        public string UniqueName { get; set; }
        public string Address { get; set; }
        public string EmailAddress { get; set; }
        public string Gender { get; set; }
        public byte[] Data { get; internal set; }
    }
}
