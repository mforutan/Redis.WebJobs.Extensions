﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings.Path;
using Microsoft.Azure.WebJobs.Host.Bindings;
using StackExchange.Redis;

namespace Redis.WebJobs.Extensions.Bindings
{
    internal class RedisEntity
    {
        private IReadOnlyDictionary<string, object> _bindingData;
        private readonly BindingTemplate _channelOrKeyPath;

        public RedisEntity(RedisAccount account, BindingTemplate channelOrKeyPath, Mode mode, IReadOnlyDictionary<string, object> bindingData)
        {
            Account = account;
            Mode = mode;
            _channelOrKeyPath = channelOrKeyPath;
            _bindingData = bindingData;
        }

        public RedisAccount Account { get; set; }
        public Mode Mode { get; set; }

        public string ChannelOrKey
        {
            get { return _channelOrKeyPath.Bind(_bindingData); }
        }

        public void SetBindingData(Type contractType, object value)
        {
            var provider = BindingDataProvider.FromType(contractType);
            _bindingData = provider.GetBindingData(value);
        }
        
        public Task SendAsync(string message)
        {
            return Account.RedisDb.PublishAsync(ChannelOrKey, message);
        }
        public async Task SetAsync(string value)
        {
            await Account.RedisDb.StringSetAsync(ChannelOrKey, value, null, When.Always, CommandFlags.None);
        }

        public async Task<string> GetAsync()
        {
            RedisValue value = await Account.RedisDb.StringGetAsync(ChannelOrKey, CommandFlags.None);
            if (!value.HasValue)
            {
                return null;
            }
            else
            {
                return (string)value;
            }
        }
    }
}
