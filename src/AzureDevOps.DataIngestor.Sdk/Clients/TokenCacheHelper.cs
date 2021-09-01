﻿using Microsoft.Identity.Client;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace AzureDevOps.InteractiveLogin
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-net-token-cache-serialization
    /// </summary>
    public static class TokenCacheHelper
    {
        public static void EnableSerialization(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccess(BeforeAccessNotification);
            tokenCache.SetAfterAccess(AfterAccessNotification);
        }

        /// <summary>
        /// Path to the token cache. Note that this could be something different for instance for MSIX applications:
        /// private static readonly string CacheFilePath =
        /// $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\{AppName}\msalcache.bin";
        /// </summary>
        public static readonly string CacheFilePath = Assembly.GetExecutingAssembly().Location + ".msalcache.bin3";

        private static readonly object FileLock = new object();

        private static void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (FileLock)
            {
                args.TokenCache.DeserializeMsalV3(File.Exists(CacheFilePath) ? ProtectedData.Unprotect(File.ReadAllBytes(CacheFilePath), null, DataProtectionScope.CurrentUser) : null);
            }
        }

        private static void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                lock (FileLock)
                {
                    // reflect changesgs in the persistent store
                    File.WriteAllBytes(CacheFilePath, ProtectedData.Protect(args.TokenCache.SerializeMsalV3(), null, DataProtectionScope.CurrentUser));
                }
            }
        }
    }
}
