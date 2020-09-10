using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFramework.BulkOperations.Tests
{
    [Table("EmployeeData")]
    public class EmployeeDataEntity
    {
        [Key]
        public string EmployeeId { get; set; }

        public string Data { get; set; }
    }
}
