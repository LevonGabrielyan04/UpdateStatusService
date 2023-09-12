namespace UpdateStatusService.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public int ServiceId { get; set; }
        public int Account { get; set; }
        public byte Status { get; set; }
        public int? ProviderTransactionId { get; set; }
        public byte ProviderStatus { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public int TryCount { get; set; }
    }
}
