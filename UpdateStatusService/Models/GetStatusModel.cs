namespace UpdateStatusService.Models
{
    public class GetStatusModel
    {
        public int TransactionId { get; set; }
        public int Status { get; set; }
        public GetStatusModel(int transactionId, int status)
        {
            TransactionId = transactionId;
            Status = status;
        }
    }
}
