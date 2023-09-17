using System.Text.Json.Serialization;

namespace BlazorServerBillTracker.Data.Models
{
    public class BillPeriod
    {
        public int Id { get; set; }
        public string? PeriodName { get; set; }
        [JsonIgnore]
        public List<Bill>? Bills { get; set; }
    }
}
