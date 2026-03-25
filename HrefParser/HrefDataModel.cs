namespace HrefParser
{
    public enum Status
    {
        Pending,
        InProgress,
        Completed,
        Failed
    }
    public class HrefDataModel : BaseViewModel
    {
        //ссылка, заголовок сайта по этой ссылке и текущий статус запроса этой ссылки
        public Uri Href
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(Href));
            }
        }
        public string SiteName
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(SiteName));
            }
        }
        public Status Status
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(Status));
            }
        }
    }
}