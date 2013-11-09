using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem.Common
{
	public class BufferedList<T>
	{
		private List<T> _front = new List<T>();
		private List<T> _back = new List<T>();

        public List<T> Front { get { return _front; } }

		public int Count { get { return _front.Count; } }
		public T this[int _index] { get { return _front[_index]; } }

		public void Swap()
		{
			List<T> _temp = _front;
			_front = _back;
			_back = _temp;
		}

		public void ClearFront()
		{
			_front.Clear();
		}

		public void Add(T _t)
		{
			_back.Add(_t);
		}

        public void AddRange(IEnumerable<T> range)
        {
            _back.AddRange(range);
        }

        public IEnumerator<T> GetEnumerator() { return _front.GetEnumerator(); }
	}
}
