using System;

namespace ControllerMicrounde
{
    public abstract class Stare
    {
        public abstract void Deschide_usa(Context context);
        public abstract void Inchide_usa(Context context);
        public abstract void Gateste(Context context);
        public abstract void Tick_ceas(Context context);
    }
}