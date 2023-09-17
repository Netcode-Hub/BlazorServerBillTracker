using BlazorServerBillTracker.Data.Models;
namespace BlazorServerBillTracker.Data.Services
{
    public interface IBillService
    {
        Task<(int code, string message)> AddBillAsync(Bill bill);
        Task<(int code, string message)> UpdateBillAsync(Bill bill);
        Task<Bill> GetBillAsync(int id);
        Task<List<Bill>> GetBillsAsync();
        Task<(int code, string message)> DeleteBillAsync(int id);


        //Period
        Task<string> AddPeriod(BillPeriod period);
        Task<List<BillPeriod>> GetPeriodsAsync();


        // Manage Bill
        Task<List<DueBill>> GetDueBillsAsync();
        Task<List<BillHistory>> GetBillsHistoryAsync();
        Task<bool> PayBillAsync(int billid, DateTime date);
    }
}
