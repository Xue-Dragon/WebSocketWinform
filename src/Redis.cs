using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSocket
{
    public class Redis
    {
        public static ServiceStack.Redis.RedisClient client6379 = new ServiceStack.Redis.RedisClient("127.0.0.1", 6379);
        public static ServiceStack.Redis.RedisClient client6380 = new ServiceStack.Redis.RedisClient("127.0.0.1", 6380);//slaveof 6379

        public static void Save(string key, string value)
        {
            client6379.Set<string>(key, value);
        }
        public static void Save(string key, object value)
        {
            client6379.Set<object>(key, value);
        }
        public static string Read(string key)
        {
            return client6380.Get<string>(key);
        }
    }
}
