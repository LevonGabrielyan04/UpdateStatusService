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
        static readonly int[] ProviderStatusCodes = new int[] { 0, 100, 200 };
        public static async Task<HttpResponseMessage> doTheWork()
        {
            try
            {
                int account = 3749642;
                int amount = 500;
                int serviceId = 123;
                int agentTransactionId;

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"https://localhost:7207/Partners/Check?serviceId={serviceId}&amount={amount}&account={account}");
                    if (response.Content.ReadFromJsonAsync<CheckModel>().Result.PaymentAcceptable)
                    {
                        Transaction transaction = new Transaction();
                        transaction.Account = account;
                        transaction.Amount = amount;
                        transaction.CreateDate = DateTime.Now;
                        transaction.ModifyDate = null;
                        transaction.ProviderStatus = 0;
                        transaction.ProviderTransactionId = null;
                        transaction.ServiceId = serviceId;
                        transaction.Status = 0;
                        transaction.TryCount = 0;
                        _context.Transactions.Add(transaction);
                        _context.SaveChanges();
                        agentTransactionId = transaction.Id;
                        response = await client.GetAsync($"https://localhost:7207/Partners/Pay?serviceId={serviceId}&amount={amount}&account={account}&agentTransactionId={agentTransactionId}");
                        transaction.ProviderTransactionId = response.Content.ReadFromJsonAsync<PayModel>().Result.TransactionId;
                        _context.Update(transaction);
                        _context.SaveChanges();
                    }
                    else
                    {
                        return new HttpResponseMessage(HttpStatusCode.BadRequest);
                    }
                }
                return await UpdateStatuses();
            }
            catch (Exception)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }
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
                            byte providerStatus = (byte)response.Content.ReadFromJsonAsync<GetStatusModel>().Result.Status;
                            int providerTransactionId = response.Content.ReadFromJsonAsync<GetStatusModel>().Result.TransactionId;
                            Transaction transaction = _context.Transactions.Find(item.Id);
                            transaction.ProviderStatus = providerStatus;
                            transaction.ProviderTransactionId = providerTransactionId;
                            transaction.Status = (byte)Array.IndexOf(ProviderStatusCodes, providerStatus);
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