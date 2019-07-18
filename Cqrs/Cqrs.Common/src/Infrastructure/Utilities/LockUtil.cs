using System;
using System.Collections;

namespace Cqrs.Infrastructure.Utilities
{
    public static class LockUtil
    {
        private class LockObject
        {
            public int Counter { get; set; }
        }

        private static readonly Hashtable LockPool = new Hashtable();

        public static void Lock(object key, Action action)
        {
            var lockObj = GetLockObject(key);
            try {
                lock (lockObj) {
                    action();
                }
            }
            finally {
                ReleaseLockObject(key, lockObj);
            }
        }

        private static void ReleaseLockObject(object key, LockObject lockObj)
        {
            lockObj.Counter--;
            lock (LockPool) {
                if (lockObj.Counter == 0) {
                    LockPool.Remove(key);
                }
            }
        }
        private static LockObject GetLockObject(object key)
        {
            lock (LockPool) {
                var lockObj = LockPool[key] as LockObject;
                if (lockObj == null) {
                    lockObj = new LockObject();
                    LockPool[key] = lockObj;
                }
                lockObj.Counter++;
                return lockObj;
            }
        }
    }
}
