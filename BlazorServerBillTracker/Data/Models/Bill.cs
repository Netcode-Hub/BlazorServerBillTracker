using System.ComponentModel.DataAnnotations;

namespace BlazorServerBillTracker.Data.Models
{
    public class Bill
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }
        [Required]
        public DateTime StartingDate { get; set; } = new();
        public bool Active { get; set; }
        public BillPeriod? BillPeriod { get; set; }
        public int BillPeriodId { get; set; }
    }
}
