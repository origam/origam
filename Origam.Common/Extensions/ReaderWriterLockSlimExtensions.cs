using System;
using System.Threading;

namespace Origam.Extensions
{
    public static class ReaderWriterLockSlimExtensions
    {
        
        public static void RunWriter(this ReaderWriterLockSlim rwLock, Action action)
        {
            rwLock.EnterWriteLock();
            try
            {
                action();
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }
        
        public static T RunWriter<T>(this ReaderWriterLockSlim rwLock, Func<T> func)
        {
            rwLock.EnterWriteLock();
            try
            {
                return func();
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }
        
        public static void RunReader(this ReaderWriterLockSlim rwLock,Action action)
        {
            rwLock.EnterReadLock();
            try
            {
                action();
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }
        
        public static T RunReader<T>(this ReaderWriterLockSlim rwLock, Func<T> func)
        {
            rwLock.EnterReadLock();
            try
            {
                return func();
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }
        
        
    }
}