namespace FlexKidsConnection
{
    using System;
    using System.Collections.Specialized;

    public interface IWeb : IDisposable
    {
        byte[] PostValues(string address, NameValueCollection data);

        string DownloadPageAsString(string address);
    }
}