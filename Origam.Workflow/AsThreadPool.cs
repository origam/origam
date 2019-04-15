#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections;
using System.Threading;
using System.Globalization;
using System.Security.Principal;

namespace Origam.Workflow
{
    public sealed class AsThreadPool : IDisposable
    {
        private readonly Queue _workItemsQueue = Queue.Synchronized(new Queue());
        private readonly ArrayList _workerThreads = new ArrayList();
        private const int NumberOfThreads = 10;
        private readonly AutoResetEvent _newWorkItemsAvailableWait = new AutoResetEvent(false);
        private volatile bool _disposed;

        public AsThreadPool()
        {
            for (int i = 0; i < NumberOfThreads; i++)
            {
                _workerThreads.Add(new WorkerThread());
            }
            Thread t = new Thread(new ThreadStart(MainExecutionThread));

			t.IsBackground = true;
			t.Start();
        }

        public void QueueUserWorkItem(MulticastDelegate workItem)
        {
            _workItemsQueue.Enqueue(new WorkQueueItem(workItem, SecurityManager.CurrentPrincipal,
													  Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture));
            _newWorkItemsAvailableWait.Set();
        }

        public IAsyncResult QueueUserWorkItemResult(MulticastDelegate workItem, AsyncCallback asyncCallback)
        {
            CustomAsyncResult asyncResult = new CustomAsyncResult(asyncCallback);
            _workItemsQueue.Enqueue(new WorkQueueItem(workItem, asyncResult, SecurityManager.CurrentPrincipal,
													  Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture));
            _newWorkItemsAvailableWait.Set();
            return asyncResult;
        }

        public static object RetreiveUserWorkItemResult(IAsyncResult asyncResult)
        {
            CustomAsyncResult custAsyncResult = asyncResult as CustomAsyncResult;
            if (custAsyncResult == null)
            {
                throw new InvalidOperationException(ResourceUtils.GetString("ErrorUnexpectedIAsyncResult"));
            }
            (custAsyncResult as IAsyncResult).AsyncWaitHandle.WaitOne();
            if (custAsyncResult.ExceptionOccured)
            {
                throw custAsyncResult.Exception;
            }
            return custAsyncResult.Result;
        }


        private void MainExecutionThread()
        {
            while (true)
            {
                
                
                while (_workItemsQueue.Count > 0)
                {
                    if(_disposed) return;

                    WorkQueueItem workItem = _workItemsQueue.Dequeue() as WorkQueueItem;

					WorkerThread availableThread = null;
					foreach(WorkerThread x in _workerThreads)
					{
						if(x.IsAvailable)
						{
							availableThread = x;
						}
					}

                    if (availableThread == null)
                    {
						availableThread = new WorkerThread();
						_workerThreads.Add(availableThread);
                    }

					availableThread.ExecuteWorkItem(workItem);
                }

                if (_disposed) return;
                _newWorkItemsAvailableWait.WaitOne();
                if (_disposed) return;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            DisposeImpl();
        }

        ~AsThreadPool()
        {
            DisposeImpl();
        }

        private void DisposeImpl()
        {
            _disposed = true;
            if (_newWorkItemsAvailableWait != null)
            {
                _newWorkItemsAvailableWait.Set(); //let background thread proceed and exit
                (_newWorkItemsAvailableWait as IDisposable).Dispose();
            }
            if (_workerThreads != null)
            {
                foreach (IDisposable workerThread in _workerThreads)
                {
                    workerThread.Dispose();
                }
            }
        }
        #endregion

        private class WorkQueueItem : IDisposable
        {
			private MulticastDelegate _delegate;
			private CustomAsyncResult _asyncResult;
			private WorkThreadExecutionMode _executionMode;
			private IPrincipal _principal;
			private CultureInfo _culture;
			private CultureInfo _uiCulutre;

            public WorkQueueItem(MulticastDelegate del, IPrincipal principal, CultureInfo culture, CultureInfo uiCulture)
            {
                Delegate = del;
				_principal = principal;
				_culture = culture;
				_uiCulutre = uiCulture;
                ExecutionMode = WorkThreadExecutionMode.Simple;
            }

            public WorkQueueItem(MulticastDelegate del, CustomAsyncResult asyncResult, IPrincipal principal,
								 CultureInfo culture, CultureInfo uiCulture)
            {
                Delegate = del;
				_principal = principal;
                ExecutionMode = WorkThreadExecutionMode.AsyncResult;
				_culture = culture;
				_uiCulutre = uiCulture;
                AsyncResult = asyncResult;
            }

			public MulticastDelegate Delegate 
			{ 
				get{return _delegate;}
				set{_delegate = value;} 
			}

			public CustomAsyncResult AsyncResult { 
				get{return _asyncResult;} 
				set{_asyncResult = value;} 
			}

			public WorkThreadExecutionMode ExecutionMode { 
				get{return _executionMode;} 
				set{_executionMode = value;} 
			}

			public IPrincipal Principal
			{ 
				get{return _principal;} 
				set{_principal = value;} 
			}
			public CultureInfo Culture
			{
				get { return _culture; }
				set { _culture = value; }
			}
			public CultureInfo UICulture
			{
				get { return _uiCulutre; }
				set { _uiCulutre = value; }
			}
			#region IDisposable Members

			public void Dispose()
			{
				_asyncResult = null;
				_delegate = null;
				_principal = null;
			}

			#endregion
		}

        private class WorkerThread : IDisposable
        {
            private Thread _thread;
            private AutoResetEvent _waitForNewTask = new AutoResetEvent(false);
            private volatile bool _isAvailable = true;
            private WorkThreadExecutionMode _executionMode;
            private MulticastDelegate _task;
            private CustomAsyncResult _asyncResult;
            private volatile bool _disposed;
            private IPrincipal _principal;
			private CultureInfo _culture;
			private CultureInfo _uiCulture;


            public WorkerThread()
            {
                _thread = new Thread(new ThreadStart(MainWorkerThread));
				_thread.IsBackground = true;
                _thread.Start();
            }

            public bool IsAvailable
            {
                get { return _isAvailable; }
            }

            private void MainWorkerThread()
            {
                while (true)
                {
                    if (_disposed) return;
                    _waitForNewTask.WaitOne();
                    if (_disposed) return;

                    try
                    {
                        System.Threading.Thread.CurrentPrincipal = _principal;
						System.Threading.Thread.CurrentThread.CurrentCulture = _culture;
						System.Threading.Thread.CurrentThread.CurrentUICulture = _uiCulture;
                        object result = _task.DynamicInvoke(null);
                        if (_executionMode == WorkThreadExecutionMode.AsyncResult)
                        {
                            _asyncResult.SetCompleted(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_executionMode == WorkThreadExecutionMode.AsyncResult)
                        {
                            _asyncResult.SetCompletedException(ex);
                        }
                    }
                    finally
                    {
                        _isAvailable = true;
                        _asyncResult = null;
                        _task = null;
                    }
                }
            }

            public void ExecuteWorkItem(WorkQueueItem workItem)
            {
                if (!_isAvailable)
                {
                    throw new InvalidOperationException("Thread is busy");
                }

				_principal = workItem.Principal;
				_culture = workItem.Culture;
				_uiCulture = workItem.UICulture;
                _executionMode = workItem.ExecutionMode;
                _task = workItem.Delegate;
                _asyncResult = workItem.AsyncResult;
                _isAvailable = false;
				
				// we won't need the workitem anymore, so we free the memory
				// otherwise the delaget will block the target object
				workItem.Dispose();

                _waitForNewTask.Set();
            }

            #region IDisposable Members

            void IDisposable.Dispose()
            {
                GC.SuppressFinalize(this);
                DisposeImpl();
            }

            ~WorkerThread()
            {
                DisposeImpl();
            }

            private void DisposeImpl()
            {
				if(_disposed) return;

                _disposed = true;
                if (_waitForNewTask != null)
                {
					try
					{
						_waitForNewTask.Set();
						(_waitForNewTask as IDisposable).Dispose();
					}
					catch{}
                }

            }
            #endregion
        }

        private enum WorkThreadExecutionMode
        {
            AsyncResult,
            Simple
        }

        private class CustomAsyncResult : IAsyncResult
        {
            private bool _isCompleted;
            private ManualResetEvent _waitHandle;
            private object _result;
            private AsyncCallback _asyncCallback;
            private Exception _ex;
            private readonly object _syncObject = new object();

            public CustomAsyncResult(AsyncCallback asyncCallback)
            {
                _waitHandle = new ManualResetEvent(false);
                _asyncCallback = asyncCallback;
            }

            #region IAsyncResult Members
            object IAsyncResult.AsyncState
            {
                get { throw new NotImplementedException(ResourceUtils.GetString("ErrorUnsupportedState")); }
            }

            WaitHandle IAsyncResult.AsyncWaitHandle
            {
                get { return _waitHandle; }
            }

            bool IAsyncResult.CompletedSynchronously
            {
                get { return false; }
            }

            bool IAsyncResult.IsCompleted
            {
                get { lock (_syncObject) { return _isCompleted; } }
            }

            #endregion

            public object Result
            {
                get
                {
                    lock (_syncObject)
                    {
                        return _result;
                    }
                }
            }

            public bool ExceptionOccured
            {
                get
                {
                    lock (_syncObject)
                    {
                        return _ex != null;
                    }
                }
            }

            public Exception Exception
            {
                get
                {
                    lock (_syncObject)
                    {
                        return _ex;
                    }
                }
            }

            public void SetCompleted(object result)
            {
                lock (_syncObject)
                {
                    _result = result;
                    _isCompleted = true;
                    _waitHandle.Set();
                    if (_asyncCallback != null)
                    {
                        _asyncCallback(this);
                    }
                }
            }

            public void SetCompletedException(Exception ex)
            {
                lock (_syncObject)
                {
                    _ex = ex;
                    _isCompleted = true;
                    _waitHandle.Set();
                    if (_asyncCallback != null)
                    {
                        _asyncCallback(this);
                    }
                }
            }

        }
    }
}
