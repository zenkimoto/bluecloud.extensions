using System;
using System.Runtime.Caching;
using BlueCloud.Extensions.Cache;
using Xunit;

namespace BlueCloud.Extensions.Tests
{
    public class DefaultCacheTests : IDisposable
    {
        DefaultCache<string> cache;

        public DefaultCacheTests()
        {
            cache = new DefaultCache<string>(new TimeSpan(4, 0, 0));
        }

        public void Dispose()
        {
            cache.Dispose();
            cache = null;
        }

        [Fact]
        public void Get_WhenItemDoesNotExist_ShouldReturnNull()
        {
            var result = cache.Get("Test");
            Assert.Null(result);
        }

        [Fact]
        public void Get_WhenItemExists_ShouldReturnCorrectValue()
        {
            cache.Set("Test", "TestValue");

            var result = cache.Get("Test");

            Assert.Equal("TestValue", result);
        }

        [Fact]
        public void Set_WhenItemExists_ShouldOverwriteExistingValue()
        {
            cache.Set("Test", "TestValue");
            cache.Set("Test", "NewTestValue");

            var result = cache.Get("Test");

            Assert.Equal("NewTestValue", result);
        }
    }
}
