namespace FlexKidsConnection
{
    using System.Collections.Specialized;
    using System.Threading.Tasks;

    public interface IWeb
    {
        Task<byte[]> PostValues(string address, NameValueCollection data);

        Task<string> DownloadPageAsString(string address);
    }
}