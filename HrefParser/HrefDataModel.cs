namespace HrefParser
{
    public enum Status
    {
        Pending,
        InProgress,
        Completed,
        Failed
    }
    public class HrefDataModel
    {
        //ссылка, заголовок сайта по этой ссылке и текущий статус запроса этой ссылки
        public Uri Href {  get; set; }
        public string SiteName { get; set; }
        public Status Status { get; set; }
    }
}