using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yenviethue.Extensions
{
    public static class ObjectExtension
    {
        public static bool IsSearchResult<T> (this T rootObj, T searchObj)
        {
            var rootProps = rootObj.GetType().GetProperties();

            foreach (var prop in rootProps)
            {
                var key = searchObj.GetType().GetProperty(prop.Name).GetValue(searchObj);
                
                if (key == null || key.ToString() == "0")
                {
                    continue;
                }

                var value = prop.GetValue(rootObj).ToString().ToLower();

                if (!value.Contains(key.ToString().ToLower()))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
