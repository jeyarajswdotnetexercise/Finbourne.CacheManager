using Xunit;
using Finbourne.CodeExcersise.CacheManager;
using Microsoft.Extensions.Configuration;
using System;

namespace Finbourne.CodeExcersise.CacheManager.Test
{
    public class CacheManagerFunctionalTest
    {
        [Fact]
        public void CacheManager_Add_CachedValues_Sucessfully_Inserted_ArbitraryTypes_CachedValues()
        {
            //Arrange
            CacheManager CacheManagerObj =new CacheManager(5);
            int actualCacheCount = 3;

            //Act 
            CacheManagerObj.AddCache(1, 1000);
            CacheManagerObj.AddCache(2, "Raj");
            CacheManagerObj.AddCache(3, "1/1/2017");
            
            var cacheResult = CacheManagerObj.GetAllCache();

            //Assert
            Assert.Equal(cacheResult.Count, actualCacheCount);

        }
        [Fact]
        public void CacheManager_Add_CachedValues_ReturnsFalse_When_CountNotMatched()
        {
            //Arrange
            CacheManager CacheManagerObj = new CacheManager(5);
            int actualCacheCount = 3;

            //Act 
            CacheManagerObj.AddCache("Name", "Raj");
            CacheManagerObj.AddCache("DOB", "1/1/2017");

            var cacheResult = CacheManagerObj.GetAllCache();

            //Assert
            Assert.NotEqual(cacheResult.Count, actualCacheCount);

        }
        [Fact]
        public void CacheManager_InsertInto_RemovedCacheMemory_When_CacheThresholdLimit_Reached_Limit()
        {

            //Arrange
            //Passing ThresholdLimit as 2 retrived from appSettings.Json
            IConfiguration Config = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build();
            var _cacheThresholdLimit = Convert.ToInt16(Config.GetSection("CacheThresholdLimit").Value);

            CacheManager CacheManagerObj = new CacheManager(_cacheThresholdLimit);
            int actualCacheCount =1;

            //Act 
            CacheManagerObj.AddCache("Name", "Raj");
            CacheManagerObj.AddCache("DOB", "1/1/2017");
            CacheManagerObj.AddCache(1, "Jey");
            CacheManagerObj.AddCache(2, "Raj");

            var removedCacheResult = CacheManagerObj.GetAllRemovedCache();

            //Assert
            Assert.Equal(removedCacheResult.Count, actualCacheCount);

        }

        [Fact]
        public void CacheManager_Rerived_EvictedDetails_RemovedCacheMemory()
        {

            //Arrange
            //Passing ThresholdLimit as 2 retrived from appSettings.Json
            IConfiguration Config = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build();
            var _cacheThresholdLimit = Convert.ToInt16(Config.GetSection("CacheThresholdLimit").Value);

            CacheManager CacheManagerObj = new CacheManager(_cacheThresholdLimit);
            int actualCacheCount = 1;

            //Act 
            CacheManagerObj.AddCache("Name", "Raj");
            CacheManagerObj.AddCache("DOB", "1/1/2017");
            CacheManagerObj.AddCache(1, "Jey");
            CacheManagerObj.AddCache("DOB2", "1/1/2019");

            var removedCacheResult = CacheManagerObj.GetAllRemovedCache();

            //Assert
            Assert.Equal(removedCacheResult.Count, actualCacheCount);
        }

    }
}