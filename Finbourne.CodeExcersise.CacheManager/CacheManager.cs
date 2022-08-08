using Microsoft.Extensions.Configuration;
namespace Finbourne.CodeExcersise.CacheManager
{
    public class CacheManager
    {
        private int _minCount;
        private readonly int _cacheThresholdLimit;

        private readonly Dictionary<object, (LinkedListNode<object> linkedNode, object value, int count)> _cache;
        private readonly Dictionary<object, (object value, DateTime timeStamp)> _removedCache;
        private readonly Dictionary<object, LinkedList<object>> _countMap;
        private object cacheLock = new object(); // Used to ensure thread-safe operations


        public CacheManager(int cacheThresholdLimit = 3)
        {

            IConfiguration Config = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build();
            _cacheThresholdLimit = Convert.ToInt16(Config.GetSection("CacheThresholdLimit").Value);
            _countMap = new Dictionary<object, LinkedList<object>> { [1] = new() };
            _cache = new Dictionary<object, (LinkedListNode<object> linkedNode, object value, int count)>(cacheThresholdLimit);
            _removedCache = new Dictionary<object, (object value, DateTime timeStamp)>(cacheThresholdLimit);
        }
        /// <summary>
        /// Retrives all the Cache values
        /// </summary>
        /// <returns></returns>
        public Dictionary<object, (LinkedListNode<object> linkedNode, object value, int count)> GetAllCache()
        {
            return _cache;

        }
        /// <summary>
        /// Retrives all the RemovedCache values
        /// </summary>
        /// <returns></returns>
        public Dictionary<object, (object value, DateTime timeStamp)> GetAllRemovedCache()
        {
            return _removedCache;

        }
        /// <summary>
        /// Retrives all the Cache values specific to the Key
        /// </summary>
        /// <returns></returns>
        public object GetCache(object key)
        {
            //return empty when key not found
            if (_cacheThresholdLimit <= 0 || !_cache.ContainsKey(key))                
                return -1;
            lock (cacheLock)
            {
                //push the recent search node to last index
                var (linkedNode, value, count) = _cache[key];
                UpdateCache(key, value, count, linkedNode);
                return value;
            }
        }
        /// <summary>
        /// Retrives  the evicted cache values specific to the Key
        /// </summary>
        /// <returns></returns>
        public Dictionary<object, object> GetCache(object key, bool isReturnEvicted) 
        {
            //Check the given Node not found in cache memory and found in RemovedCache memory
            var _recentlyRemovedCache = new Dictionary<object, object>();
            if (!_cache.ContainsKey(key) && _removedCache.ContainsKey(key) && isReturnEvicted)
            { 
                //retrun the evicted key value and evicted timestamp
               var ( value, timeStamp) = _removedCache[key];
                _recentlyRemovedCache.Add(value, timeStamp);
                return _recentlyRemovedCache;
            }
            else
            { 
                return new Dictionary<object, object>();
            }
        }
        /// <summary>
        /// Insert the Cache values into the cache memory
        /// </summary>
        /// <returns></returns>
        public void AddCache(object cachekey, object cacheValue)
        {
            //cacheThresholdLimit should be greater than zero
            if (_cacheThresholdLimit <= 0) return;

            lock (cacheLock)
            {
                //Update and push the recent search node to last index if aready cache key found in memory
                if (_cache.ContainsKey(cachekey))
                {
                    var (linkedNode, _, count) = _cache[cachekey];
                    UpdateCache(cachekey, cacheValue, count, linkedNode);
                }
                else
                {
                    //Insert into _removedCache when cacheThresholdLimit exceed the memory count
                    if (_cache.Count >= _cacheThresholdLimit)
                    {
                        var minList = _countMap[_minCount];
                        if (!_removedCache.ContainsKey(minList.Last!.Value))
                        {
                            var (removedlinkedNode, removedValue, removedCnt) = _cache[minList.Last!.Value];
                            _removedCache.Add(minList.Last!.Value, (removedValue, DateTime.UtcNow));
                        }
                        else
                        {
                            var (removedValue, removedTimestamp) = _removedCache[minList.Last!.Value];
                            UpdateRemovedNode(minList.Last!.Value, removedValue);
                        }
                        //Removing the least used node
                        _cache.Remove(minList.Last!.Value);
                        minList.RemoveLast();
                    }
                    //Inserting into cachememory
                    SaveCache(cachekey, cacheValue);
                    _minCount = 1;
                }
            }
        }

        /// <summary>
        //Inserting the key and value to cache memory
        /// </summary>
        /// <returns></returns>

        private void SaveCache(object cachekey, object cacheValue)
        {
            _cache.Add(cachekey, (_countMap[1].AddFirst(cachekey), cacheValue, 1));
            //Remove the Cache value FromRemovedCacheMemory when node added into the actuial cache memory
            RemoveCacheFromRemovedCacheMemory(cachekey);
            
        }

        /// <summary>
        //Update the timestamp for the existing removed cache memory
        /// </summary>
        /// <returns></returns>
        public void UpdateRemovedNode(object key, object value)
        {
            lock (cacheLock)
            {
                if (_removedCache.ContainsKey(key))
                    _removedCache[key] = ( value, DateTime.UtcNow);
            }
        }

        /// <summary>
        //Update the Cache index position 
        /// </summary>
        /// <returns></returns>
        public void UpdateCache(object key)
        {
            if (_cacheThresholdLimit <= 0) return;

            lock (cacheLock)
            {
                if (_cache.ContainsKey(key))
                {
                    var (linkedNode, value, count) = _cache[key];
                    UpdateCache(key, value, count, linkedNode);
                }
            }
        }

        /// <summary>
        //Update and push the recent search node to last index
        /// </summary>
        /// <returns></returns>
        private void UpdateCache(object key, object value, int count, LinkedListNode<object> linkedNode)
        {
            lock (cacheLock)
            {
                var list = _countMap[count];
                list.Remove(linkedNode);

                if (_minCount == count && list.Count == 0)
                    _minCount++;

                var newCount = count + 1;
                if (!_countMap.ContainsKey(newCount))
                    _countMap[newCount] = new LinkedList<object>();

                _countMap[newCount].AddFirst(linkedNode);
                _cache[key] = (linkedNode, value, newCount);
            }
        }

        /// <summary>
        //Remove from node from cache memory
        /// </summary>
        /// <returns></returns>
        public void RemoveCache(object key)
        {

            lock (cacheLock)
            {
                if (_cache.ContainsKey(key))
                {
                    _cache.Remove(key);
                }
            }
        }

        /// <summary>
        //Remove from  RemoveCache Memory when given key added into the cache memory
        /// </summary>
        /// <returns></returns>
        public void RemoveCacheFromRemovedCacheMemory(object key)
        {

            lock (cacheLock)
            {
                if (_removedCache.ContainsKey(key))
                {
                    _removedCache.Remove(key);
                }
            }
        }
    }
}