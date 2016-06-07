﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Hosting;
using Frapid.ApplicationState.CacheFactory;
using Frapid.Areas.SpamTrap;
using Frapid.Configuration;

namespace Frapid.Areas
{
    internal static class DnsSpamLookupHelper
    {
        private static string[] GetRblServers()
        {
            string tenant = TenantConvention.GetTenant();

            //Check RBL server list in tenant directory.
            string path = HostingEnvironment.MapPath($"/Tenants/{tenant}/Configs/RblServers.config");

            if(!File.Exists(path))
            {
                //Fallback to shared RBL server list.
                path = HostingEnvironment.MapPath($"/Resources/Configs/RblServers.config");
            }

            if(path == null ||
               !File.Exists(path))
            {
                return new[]
                       {
                           ""
                       };
            }

            string contents = File.ReadAllText(path, Encoding.UTF8);

            return contents.Split
                (
                 new[]
                 {
                     Environment.NewLine
                 },
                 StringSplitOptions.RemoveEmptyEntries);
        }

        internal static DnsSpamLookupResult IsListedInSpamDatabase(string ipAddress)
        {
            bool isLoopBack = IPAddress.IsLoopback(IPAddress.Parse(ipAddress));

            if(isLoopBack)
            {
                return new DnsSpamLookupResult();
            }

            string key = ipAddress + ".spam.check";
            var factory = new DefaultCacheFactory();

            var isListed = factory.Get<DnsSpamLookupResult>(key);

            if(isListed == null)
            {
                isListed = FromStore(ipAddress);
                factory.Add(key, isListed, DateTimeOffset.UtcNow.AddHours(2));
            }

            return isListed;
        }

        private static DnsSpamLookupResult FromStore(string ipAddress)
        {
            var lookupServers = GetRblServers();
            var reverser = new IpAddressReverser();
            var resolver = new HostEntryResolver();
            var queryable = new DnsQueryable(resolver);

            var lookup = new DnsSpamLookup(reverser, queryable, lookupServers);
            return lookup.IsListedInSpamDatabase(ipAddress);
        }
    }
}