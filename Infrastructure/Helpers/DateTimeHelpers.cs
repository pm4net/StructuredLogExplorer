using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public static class DateTimeHelpers
    {
        public static DateTimeOffset StartOfDay(this DateTimeOffset theDate)
        {
            return theDate.Date;
        }

        public static DateTimeOffset EndOfDay(this DateTimeOffset theDate)
        {
            return theDate.Date.AddDays(1).AddTicks(-1);
        }
    }
}
