﻿using System;
using System.Linq;

namespace DistrEx.Common
{
    public static class EventRaiseHelper
    {
        public static void Raise(this EventHandler eh, object sender, EventArgs e)
        {
            if (eh == null)
            {
                return;
            }

            foreach (EventHandler handler in eh.GetInvocationList().Cast<EventHandler>())
            {
                try
                {
                    handler(sender, e);
                }
                catch
                {
                }
            }
        }

        public static void Raise<T>(this EventHandler<T> eh, object sender, T e) where T : EventArgs
        {
            if (eh != null)
            {
                foreach (var handler in eh.GetInvocationList().Cast<EventHandler<T>>())
                {
                    try
                    {
                        handler(sender, e);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
