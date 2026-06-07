using System;
using System.Globalization;
using UnityEngine;
public static class DataHelper
{
    public static bool IsToday(string dateString)
    {
        DateTime parsedDate;
        if (DateTime.TryParseExact(dateString, "yyyy-MM-dd",
                                   CultureInfo.InvariantCulture,
                                   DateTimeStyles.None,
                                   out parsedDate))
        {
            return parsedDate.Date == DateTime.Now.Date;
        }
        if (DateTime.TryParse(dateString, out parsedDate))
        {
            return parsedDate.Date == DateTime.Now.Date;
        }
        return false;
    }
}