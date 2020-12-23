namespace FlexKidsScheduler
{
    using System;

    public interface IFlexKidsConnection : IDisposable
    {
        string GetSchedulePage(int id);

        string GetAvailableSchedulesPage();
    }
}