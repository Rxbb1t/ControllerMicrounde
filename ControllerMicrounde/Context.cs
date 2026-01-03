using System;
using System.Collections.Generic;

namespace ControllerMicrounde
{
    public class Context : IObservable<Stare>
    {
        private static Context? instance = null;
        private static List<IObserver<Stare>> observers = new List<IObserver<Stare>>();

        public Stare Stare_curenta { get; private set; }
        public int Timp_ramas { get; set; }

        private const int TIMP_INITIAL = 10;

        private Context()
        {
            Timp_ramas = TIMP_INITIAL;
            Stare_curenta = Stare_usa_inchisa.Instance;
        }

        public static Context Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Context();
                }
                return instance;
            }
        }

        public IDisposable Subscribe(IObserver<Stare> observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }
            return new Unsubscriber(observers, observer);
        }

        private void NotifyObservers()
        {
            foreach (var observer in observers)
            {
                observer.OnNext(Stare_curenta);
            }
        }

        public void SchimbaStare(Stare stareNoua)
        {
            Stare_curenta = stareNoua;
            NotifyObservers();
        }

        public void ResetTimp()
        {
            Timp_ramas = TIMP_INITIAL;
        }

        public void AdaugaTimp()
        {
            Timp_ramas += TIMP_INITIAL;
        }

        public void Deschide_usa()
        {
            Stare_curenta.Deschide_usa(this);
        }

        public void Inchide_usa()
        {
            Stare_curenta.Inchide_usa(this);
        }

        public void Gateste()
        {
            Stare_curenta.Gateste(this);
        }

        public void Tick_ceas()
        {
            Stare_curenta.Tick_ceas(this);
        }
    }
}