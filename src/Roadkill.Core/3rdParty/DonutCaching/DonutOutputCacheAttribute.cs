using System;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

//The MIT License (MIT)
//Copyright (c) 2011 Paul Hiles
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
//to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
//WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace DevTrends.MvcDonutCaching
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class DonutOutputCacheAttribute : ActionFilterAttribute, IExceptionFilter
    {
        private readonly IKeyGenerator _keyGenerator;
        private readonly IDonutHoleFiller _donutHoleFiller;
        private readonly IExtendedOutputCacheManager _outputCacheManager;
        private readonly ICacheSettingsManager _cacheSettingsManager;
        protected ICacheHeadersHelper CacheHeadersHelper; // the change from the Codeplex version

        private bool? _noStore;
        private CacheSettings _cacheSettings;

        public int Duration { get; set; }
        public string VaryByParam { get; set; }
        public string VaryByCustom { get; set; }
        public string CacheProfile { get; set; }
        public OutputCacheLocation Location { get; set; }
        
        public bool NoStore 
        {
            get { return _noStore ?? false; }
            set { _noStore = value; }
        }

        public DonutOutputCacheAttribute()
        {
            var keyBuilder = new KeyBuilder();

            _keyGenerator = new KeyGenerator(keyBuilder);
            _donutHoleFiller = new DonutHoleFiller(new EncryptingActionSettingsSerialiser(new ActionSettingsSerialiser(), new Encryptor()));
            _outputCacheManager = new OutputCacheManager(OutputCache.Instance, keyBuilder);
            _cacheSettingsManager = new CacheSettingsManager();
            CacheHeadersHelper = new CacheHeadersHelper();

            Duration = -1;
            Location = (OutputCacheLocation)(-1);
			VaryByParam = "";
			VaryByCustom = "";
			CacheProfile = "";
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            _cacheSettings = BuildCacheSettings();

            var cacheKey = _keyGenerator.GenerateKey(filterContext, _cacheSettings);

            if (_cacheSettings.IsServerCachingEnabled)
            {
                var cachedItem = _outputCacheManager.GetItem(cacheKey);

                if (cachedItem != null)
                {
                    filterContext.Result = new ContentResult
                    { 
                        Content = _donutHoleFiller.ReplaceDonutHoleContent(cachedItem.Content, filterContext),
                        ContentType = cachedItem.ContentType
                    };
                }
            }

            if (filterContext.Result == null)
            {
                var cachingWriter = new StringWriter(CultureInfo.InvariantCulture);

                var originalWriter = filterContext.HttpContext.Response.Output;

                filterContext.HttpContext.Response.Output = cachingWriter;

                filterContext.HttpContext.Items[cacheKey] = new Action<bool>(hasErrors =>
                {
                    filterContext.HttpContext.Items.Remove(cacheKey);

                    filterContext.HttpContext.Response.Output = originalWriter;

                    if (!hasErrors)
                    {
                        var cacheItem = new CacheItem
                        {
                            Content = cachingWriter.ToString(),
                            ContentType = filterContext.HttpContext.Response.ContentType
                        };

                        filterContext.HttpContext.Response.Write(_donutHoleFiller.RemoveDonutHoleWrappers(cacheItem.Content, filterContext));

                        if (_cacheSettings.IsServerCachingEnabled && filterContext.HttpContext.Response.StatusCode == 200)
                        {
                            _outputCacheManager.AddItem(cacheKey, cacheItem, DateTime.UtcNow.AddSeconds(_cacheSettings.Duration));
                        }
                    }
                });
            }
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            ExecuteCallback(filterContext, false);

            if (!filterContext.IsChildAction)
            {
                CacheHeadersHelper.SetCacheHeaders(filterContext.HttpContext.Response, _cacheSettings);
            }
        }

        public void OnException(ExceptionContext filterContext)
        {
            if (_cacheSettings != null)
            {
                ExecuteCallback(filterContext, true);
            }
        }

        private void ExecuteCallback(ControllerContext context, bool hasErrors)
        {
            var cacheKey = _keyGenerator.GenerateKey(context, _cacheSettings);

            var callback = context.HttpContext.Items[cacheKey] as Action<bool>;

            if (callback != null)
            {
                callback.Invoke(hasErrors);
            }
        }

        private CacheSettings BuildCacheSettings()
        {
            CacheSettings cacheSettings;

            if (string.IsNullOrEmpty(CacheProfile))
            {
                cacheSettings = new CacheSettings
                {
                    IsCachingEnabled = _cacheSettingsManager.IsCachingEnabledGlobally,
                    Duration = Duration,
                    VaryByCustom = VaryByCustom,
                    VaryByParam = VaryByParam,
                    Location = (int)Location == -1 ? OutputCacheLocation.Server : Location,
                    NoStore = NoStore
                };
            }
            else
            {
                var cacheProfile = _cacheSettingsManager.RetrieveOutputCacheProfile(CacheProfile);

                cacheSettings = new CacheSettings
                {
                    IsCachingEnabled = _cacheSettingsManager.IsCachingEnabledGlobally && cacheProfile.Enabled,
                    Duration = Duration == -1 ? cacheProfile.Duration : Duration,
                    VaryByCustom = VaryByCustom ?? cacheProfile.VaryByCustom,
                    VaryByParam = VaryByParam ?? cacheProfile.VaryByParam,
                    Location = (int)Location == -1 ? ((int)cacheProfile.Location == -1 ? OutputCacheLocation.Server : cacheProfile.Location) : Location,
                    NoStore = _noStore.HasValue ? _noStore.Value : cacheProfile.NoStore
                };
            }

            if (cacheSettings.Duration == -1)
            {
                throw new HttpException("The directive or the configuration settings profile must specify the 'duration' attribute.");
            }

            if (cacheSettings.Duration < 0)
            {
                throw new HttpException("The 'duration' attribute must have a value that is greater than or equal to zero.");
            }

            return cacheSettings;
        }        
    }
}