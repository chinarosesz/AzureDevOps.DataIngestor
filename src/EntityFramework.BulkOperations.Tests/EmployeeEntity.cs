using System;
using System.ComponentModel.DataAnnotations;

namespace EntityFramework.BulkOperations.Tests
{
    public class EmployeeEntity
    {
        [Key]
        public Guid EmployeeId { get; set; }
        public string First { get; set; }
        public string Last { get; set; }
        public string Team { get; set; }
        public string Organization { get; set; }
        public DateTime HiredDate { get; set; }
        public string UniqueName { get; set; }
        public string Address { get; set; }
        public string EmailAddress { get; set; }
        public string Gender { get; set; }
        public string Data { get; internal set; }
    }
}
