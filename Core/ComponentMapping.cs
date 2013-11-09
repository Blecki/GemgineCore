using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem
{
    public class ComponentMapping<T, C> : IEnumerable<C>
    {
        private Dictionary<T, List<C>> _data = new Dictionary<T, List<C>>();
        
        private List<C> getComponentList(T t)
        {
            if (!_data.ContainsKey(t)) _data.Add(t, new List<C>());
            return _data[t];
        }

        public bool ContainsKey(T t) { return _data.ContainsKey(t); }

        public List<C> this[T t] { get { return _data[t]; } }

        public void Add(T t, C c)
        {
            getComponentList(t).Add(c);
        }

        public void Remove(T t)
        {
            if (_data.ContainsKey(t)) _data.Remove(t);
        }

        public IEnumerator<C> GetEnumerator()
        {
            foreach (var entity in _data)
                foreach (var item in entity.Value)
                    yield return item;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var entity in _data)
                foreach (var item in entity.Value)
                    yield return item;
        }
    }
}
