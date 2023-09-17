using BlazorServerBillTracker.Data.DatabaseConfiguration;
using BlazorServerBillTracker.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorServerBillTracker.Data.Services
{
    public class BillService : IBillService
    {
        private readonly AppDbContext appDbContext;
        public BillService(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        public async Task<(int code, string message)> AddBillAsync(Bill bill)
        {
            try
            {
                appDbContext.Bills.Add(bill);
                await appDbContext.SaveChangesAsync();
                return (1, "Bill Created");
            }
            catch (Exception ex)
            {
                return (-1, ex.Message);
            }
        }

        public async Task<Bill> GetBillAsync(int id)
        {
            try
            {
                var bill = await appDbContext.Bills.FirstOrDefaultAsync(_ => _.Id == id);
                return bill ?? null!;
            }
            catch (Exception) { return null!; }

        }
        public async Task<List<Bill>> GetBillsAsync()
        {
            var bills = await appDbContext.Bills.Include(_ => _.BillPeriod).ToListAsync();
            return bills ?? null!;
        }

        public async Task<(int code, string message)> DeleteBillAsync(int id)
        {
            try
            {
                var bill = await appDbContext.Bills.FirstOrDefaultAsync(_ => _.Id == id);
                if (bill == null) return (-1, "Bill not found");

                appDbContext.Bills.Remove(bill);
                await appDbContext.SaveChangesAsync();
                return (1, "Bill Deleted");
            }
            catch (Exception ex)
            {
                return (-2, ex.Message);
            }
        }

        public async Task<(int code, string message)> UpdateBillAsync(Bill bill)
        {
            try
            {
                var uBill = await appDbContext.Bills.FirstOrDefaultAsync(_ => _.Id == bill.Id);
                if (bill is null) return (-1, "Bill not found");

                uBill.Name = bill.Name;
                uBill.StartingDate = bill.StartingDate;
                uBill.BillPeriodId = bill.BillPeriodId;
                uBill.Active = bill.Active;
                uBill.Amount = bill.Amount;
                await appDbContext.SaveChangesAsync();
                return (1, "Bill Updated");
            }
            catch (Exception ex)
            {
                return (-2, ex.Message);
            }
        }



        // Manage Periods
        public async Task<string> AddPeriod(BillPeriod period)
        {
            appDbContext.BillPeriods.Add(period);
            await appDbContext.SaveChangesAsync();
            return "Success";
        }
        public async Task<List<BillPeriod>> GetPeriodsAsync() => await appDbContext.BillPeriods.ToListAsync();


        // Prepare all due bills into DuBills Table which is awaiting for payments.
        public async Task<List<DueBill>> GetDueBillsAsync()
        {
            var getAlActivelBills = await appDbContext.Bills.Include(_ => _.BillPeriod).Where(_ => _.Active).ToListAsync();
            if (getAlActivelBills.Any())
            {
                // Get and prepare Monthly bills.
                var getAllMonthlyBills = getAlActivelBills.Where(_ => _.BillPeriod!.PeriodName!.Equals("Monthly")).ToList();
                if (getAllMonthlyBills.Any())
                {
                    foreach (var bill in getAllMonthlyBills)
                    {
                        if (DateTime.Now.Date == bill.StartingDate.AddMonths(1))
                        {
                            // this means the bill is due to be paid, sp prepare the bill and save it to the DueBills Table
                            //First check and see if the bill has being already prepared from the DueBills Table.

                            var checkBill = await appDbContext.DueBills.FirstOrDefaultAsync(_ => _.BillId == bill.Id && _.DueDate == bill.StartingDate!.AddMonths(1).Date);
                            if (checkBill is null)
                            {
                                var newDueBill = new DueBill()
                                {
                                    BillId = bill.Id,
                                    Name = bill.Name,
                                    Amount = bill.Amount,
                                    DueDate = bill.StartingDate.AddMonths(1),
                                    BillType = bill.BillPeriod!.PeriodName,
                                    Paid = false
                                };
                                appDbContext.DueBills.Add(newDueBill);
                                await appDbContext.SaveChangesAsync();

                                // update the deafult bill starting date to next month.

                                var defaultBill = await appDbContext.Bills.FirstOrDefaultAsync(_ => _.Id == bill.Id);
                                if (defaultBill is not null)
                                {
                                    defaultBill.StartingDate = defaultBill.StartingDate!.AddMonths(1);
                                    await appDbContext.SaveChangesAsync();
                                }
                            }
                        }
                    }
                }

                // Get and prepare Weekly bills.
                var getAllWeeklyBills = getAlActivelBills.Where(_ => _.BillPeriod!.PeriodName!.Equals("Weekly")).ToList();
                if (getAllWeeklyBills.Any())
                {
                    foreach (var bill in getAllWeeklyBills)
                    {
                        DateTime dt = bill.StartingDate.AddDays(7);
                        if (DateTime.Now.Date == dt.Date)
                        {
                            // this means the bill is due to be paid, so prepare the bill and save it to the DueBills Table
                            //First check and see if the bill has being already prepared from the DueBills Table.

                            var checkBill = await appDbContext.DueBills.FirstOrDefaultAsync(_ => _.BillId == bill.Id && _.DueDate == bill.StartingDate!.AddDays(7).Date);
                            if (checkBill is null)
                            {
                                var newDueBill = new DueBill()
                                {
                                    BillId = bill.Id,
                                    Name = bill.Name,
                                    Amount = bill.Amount,
                                    DueDate = bill.StartingDate!.AddDays(7),
                                    BillType = bill.BillPeriod!.PeriodName,
                                    Paid = false
                                };
                                appDbContext.DueBills.Add(newDueBill);
                                await appDbContext.SaveChangesAsync();

                                // update the deafult bill starting date to next month.

                                var defaultBill = await appDbContext.Bills.FirstOrDefaultAsync(_ => _.Id == bill.Id);
                                if (defaultBill is not null)
                                {
                                    defaultBill.StartingDate = defaultBill.StartingDate!.AddDays(7);
                                    await appDbContext.SaveChangesAsync();
                                }
                            }
                        }
                    }
                }
            }
            return await appDbContext.DueBills.ToListAsync();
        }

        public async Task<bool> PayBillAsync(int billid, DateTime d)
        {
            var result = await appDbContext.DueBills.FirstOrDefaultAsync(_ => _.BillId == billid && _.DueDate.Date == d.Date);
            if (result is not null)
            {
                result.Paid = true;
                await appDbContext.SaveChangesAsync();

                // move to the history table
                var newHistory = new BillHistory()
                {
                    BillId = result.BillId,
                    Amount = result.Amount,
                    DueDate = result.DueDate,
                    Paid = result.Paid,
                    BillType = result.BillType,
                    Name = result.Name
                };

                appDbContext.BillsHistory.Add(newHistory);
                await appDbContext.SaveChangesAsync();

                //remove from due bills after sent to history
                appDbContext.DueBills.Remove(result);
                await appDbContext.SaveChangesAsync();

            }
            return true;
        }

        public async Task<List<BillHistory>> GetBillsHistoryAsync()
        {
            return await appDbContext.BillsHistory.ToListAsync();
        }




    }
}
