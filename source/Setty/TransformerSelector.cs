using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Setty.Engines.MvcRazor;
using System.IO;

namespace Setty
{
    public class TransformerSelector
    {
        private List<ITransformer> _transformers;

        public TransformerSelector()
        {
            _transformers = GetInstancesOfImplementingTypes<ITransformer>().ToList();
        }

        public List<ITransformer> AllTransformers
        {
            get { return _transformers; }
        }

        /// <summary>
        /// Select approriate transformer by file extention
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public ITransformer Select(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name can't be empty");

            var extention = Path.GetExtension(fileName).Replace(".", String.Empty);

            return AllTransformers.Single(x => x.ConfigExtention == extention);
        }

        /// <summary>
        /// Get all instances of specific type in current assembly
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <returns>list of instances</returns>
        private static IEnumerable<T> GetInstancesOfImplementingTypes<T>()
        {
            AppDomain app = AppDomain.CurrentDomain;
            Assembly[] ass = app.GetAssemblies();
            Type[] types;
            Type targetType = typeof(T);

            foreach (Assembly a in ass)
            {
                types = a.GetTypes();
                foreach (Type t in types)
                {
                    if (t.IsInterface) continue;
                    if (t.IsAbstract) continue;
                    foreach (Type iface in t.GetInterfaces())
                    {
                        if (!iface.Equals(targetType)) continue;
                        yield return (T)Activator.CreateInstance(t);
                        break;
                    }
                }
            }
        }

    }
}
