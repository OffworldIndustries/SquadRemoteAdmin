using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Squad.Admin.Rcon
{
    public class SquadRconBase : IDisposable
    {
        /// <summary>
        /// To check whether dispose method was called before.
        /// </summary>
        protected bool IsDisposed { get; set; }
        /// <summary>
        /// Disposes all the resources used by this instance.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {

        }
        /// <summary>
        /// Throw <see cref="ObjectDisposedException"/> if this instance is already disposed.
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
        }

        internal T Invoke<T>(Func<T> method, int attempts, AttemptCallback attemptcallback, bool throwExceptions) where T : class
        {
            int AttemptCounter = 0;
            while (true)
            {
                try
                {
                    AttemptCounter++;
                    if (attemptcallback != null)
                    {
                        ThreadPool.QueueUserWorkItem(x => attemptcallback(AttemptCounter));
                    }
                    T reply = method();
                    return reply;
                }
                catch (Exception ex)
                {
                    if (AttemptCounter >= attempts)
                        if (throwExceptions)
                            throw;
                        else
                            return null;
                }

            }
        }

        /// <summary>
        /// Disposes all the resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SquadRconBase()
        {
            Dispose(false);
        }
    }
}
