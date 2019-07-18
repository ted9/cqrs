﻿using System;
using System.Runtime.ConstrainedExecution;

namespace Cqrs
{
    public abstract class DisposableObject : CriticalFinalizerObject, IDisposable
    {
        ~DisposableObject()
        {
            this.Dispose(false);
        }

        protected abstract void Dispose(bool disposing);

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
