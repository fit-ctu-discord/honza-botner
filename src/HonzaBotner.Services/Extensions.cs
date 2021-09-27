using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using HonzaBotner.Services.Contract.Dto;

namespace HonzaBotner.Services
{
    internal static class Extensions
    {
        public static string GetQueryString(this NameValueCollection queryCollection) => string.Join('&',
                   queryCollection.AllKeys.Select(k => $"{k}={HttpUtility.UrlEncode(queryCollection[k])}"));
    }
}
