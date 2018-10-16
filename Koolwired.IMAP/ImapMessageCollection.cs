using System.Collections.Generic;

namespace Koolwired.Imap
{
    public class ImapMessageCollection : IList<ImapMessage>
    {
        #region Private Variables
        List<ImapMessage> store = new List<ImapMessage>();
        #endregion

        #region Public Properties
        public bool IsFixedSize
        {
            get { return false; }
        }
        public bool IsReadOnly
        {
            get { return false; }
        }
        public ImapMessage this[int index]
        {
            get            {                return store[index];            }
            set            {                store[index] = value;            }
        }
        public int Count
        {
            get { return store.Count; }
        }
        #endregion

        #region Public Methods
        
        public void Add(ImapMessage item)
        {
            store.Add(item);
        }
        public void Clear()
        {
            store.Clear();
        }
        /// <summary>
        /// Searches for the specified ImapMessage and returns the zero based index of the first occurrence within the range of elements in the ImapMessageCollection that
        /// extends from the specified index to the last element.
        /// </summary>
        /// <param name="item">The ImapMessage object to locate.</param>
        /// <param name="index">The zero based starting index of the search.</param>
        /// <returns>Returns a zero based index indicating the position of the item within the ImapMessageCollection.</returns>
        public int IndexOf(ImapMessage item)
        {
            return store.IndexOf(item);
        }
        /// <summary>
        /// Searches for the specified ImapMessage and returns the zero based index of the first occurrence within the range of elements in the ImapMessageCollection that
        /// extends from the specified index to the last element.
        /// </summary>
        /// <param name="item">The ImapMessage object to locate.</param>
        /// <param name="index">The zero based starting index of the search.</param>
        /// <returns>Returns a zero based index indicating the position of the item within the ImapMessageCollection.</returns>
        public int IndexOf(ImapMessage item, int index)
        {
         return   store.IndexOf(item, index);
        }
        /// <summary>
        /// Searches for the specified ImapMessage and returns the zero based index of the first occurrence within the range of elements in the ImapMessageCollection that
        /// starts at the specified index and contains the specified number of elements.
        /// </summary>
        /// <param name="item">The ImapMessage object to locate.</param>
        /// <param name="index">The zero based starting index of the search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <returns>Returns a zero based index indicating the position of the item within the ImapMessageCollection.</returns>
        public int IndexOf(ImapMessage item, int index, int count)
        {
            return store.IndexOf(item, index, count);
        }
        public bool Remove(ImapMessage item)
        {
            return store.Remove(item);
        }
        public void RemoveAt(int index)
        {
            store.RemoveAt(index);
        }
        public void CopyTo(ImapMessage[] array, int index)
        {
            store.CopyTo(array, index);
        }
        #endregion

        #region Private Methods
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)store).GetEnumerator();
        }
        #endregion

        #region IList<ImapMessage> Members


        public void Insert(int index, ImapMessage item)
        {
            store.Insert(index, item);
        }

        #endregion

        #region ICollection<ImapMessage> Members


        public bool Contains(ImapMessage item)
        {
            return store.Contains(item);
        }

        #endregion

        #region IEnumerable<ImapMessage> Members

        IEnumerator<ImapMessage> IEnumerable<ImapMessage>.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
