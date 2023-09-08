using Azure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UpdateStatusService.Models;

namespace UpdateStatusService
{
    public class Routes
    {
        static AppDbContext _context = new AppDbContext();
        public static int TTL;
        public static async Task<HttpResponseMessage> UpdateStatuses()// /updatestatuses
        {
            await Task.Delay(TTL);
            return await UpdateStatusImidiantly();
        }
        public static async Task<HttpResponseMessage> UpdateStatusImidiantly()
        {
            try
            {
                List<Transaction> transactions = _context.Transactions.Where(t => t.Status == 0).ToList();
                using (HttpClient client = new HttpClient())
                {
                    foreach (var item in transactions)
                    {

                        HttpResponseMessage response = await client.GetAsync($"https://localhost:7207/Partners/GetStatus?agentId={item.Id}");
                        if (response.IsSuccessStatusCode)
                        {
                            byte status = Convert.ToByte(response.Content.ReadFromJsonAsync<GetStatusModel>().Result.Status);
                            Transaction transaction = _context.Transactions.Find(item.Id);
                            transaction.Status = status;
                            transaction.ModifyDate = DateTime.Now;
                            transaction.TryCount++;
                            _context.Update(transaction);
                        }
                        else
                        {
                            Transaction transaction = _context.Transactions.Find(item.Id);
                            transaction.ModifyDate = DateTime.Now;
                            transaction.TryCount++;
                            _context.Update(transaction);
                        }
                    }
                    _context.SaveChanges();
                }
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }
    }
}