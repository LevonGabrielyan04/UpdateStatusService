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
        public static async Task<HttpResponseMessage> doTheWork(int account,int amount,int serviceId)
        {
            try
            {

                using (HttpClient client = new HttpClient())
                {
                    var values = new Dictionary<string, string>
                    {
                        { "serviceId", serviceId.ToString() },
                        { "amount", amount.ToString() },
                        { "account", account.ToString() }
                    };
                    var content = new FormUrlEncodedContent(values);

                    HttpResponseMessage response = await client.PostAsync($"https://localhost:7207/Partners/Check",content);
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
                        values.Add("agentTransactionId",transaction.Id.ToString());
                        response = await client.PostAsync($"https://localhost:7207/Partners/Pay",content);
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
                        var values = new Dictionary<string, string>
                        {
                            { "agentId", item.Id.ToString() },
                        };
                        var content = new FormUrlEncodedContent(values);
                        HttpResponseMessage response = await client.PostAsync($"https://localhost:7207/Partners/GetStatus",content);
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