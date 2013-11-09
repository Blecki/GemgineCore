using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem.Common
{
	public class DelayList<T> : IEnumerable<T>
	{
		private List<T> _front = new List<T>();
		private List<T> _back = new List<T>();

		public int Count { get { return _front.Count; } }
		public T this[int _index] { get { return _front[_index]; } }

		public void Collapse()
		{
            _front.AddRange(_back);
            _back.Clear();
		}

		public void ClearFront()
		{
			_front.Clear();
		}

		public void Add(T _t)
		{
			_back.Add(_t);
		}

        public IEnumerator<T> GetEnumerator()
        {
            return _front.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _front.GetEnumerator();
        }
    }
}
