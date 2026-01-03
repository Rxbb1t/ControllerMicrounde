using System;
using System.Collections.Generic;

namespace ControllerMicrounde
{
    public class Unsubscriber : IDisposable
    {
        private List<IObserver<Stare>> _observers;
        private IObserver<Stare> _observer;

        public Unsubscriber(List<IObserver<Stare>> observers, IObserver<Stare> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
            {
                _observers.Remove(_observer);
            }
        }
    }
}