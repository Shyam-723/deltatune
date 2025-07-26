using System;
using System.Diagnostics;
using System.Threading;

namespace DeltaTune
{
    // Taken from https://stackoverflow.com/questions/70089516/prevent-my-windows-application-to-run-multiple-times
    public class SingleInstance : IDisposable
    {
        private Mutex  mutex;
  
        // Private default constructor to suppress uncontrolled instantiation.
        private SingleInstance() {}
  
        public SingleInstance(string mutexName)
        {
            if(string.IsNullOrWhiteSpace(mutexName))
                throw new ArgumentNullException("mutexName");
    
            mutex = new Mutex(false, mutexName);         
        }

        ~SingleInstance()
        {
            Dispose(false);
        }
    
        public bool IsRunning
        {
            get
            {
                // requests ownership of the mutex and returns true if succeeded
                return !mutex.WaitOne(1, true);
            }    
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if(mutex != null) mutex.Close();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                mutex = null;
            }
        }
    }
}