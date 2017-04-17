using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Core
{
    public interface IEmailNotifier
    {
        Task Notify(string subject, string content);
    }
}
