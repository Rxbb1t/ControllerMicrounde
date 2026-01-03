using System;

namespace ControllerMicrounde
{
    public class Stare_usa_deschisa : Stare
    {
        private static Stare_usa_deschisa? instance = null;

        private Stare_usa_deschisa() { }

        public static Stare_usa_deschisa Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Stare_usa_deschisa();
                }
                return instance;
            }
        }

        public override void Deschide_usa(Context context)
        {
            // Usa este deja deschisa
        }

        public override void Inchide_usa(Context context)
        {
            context.SchimbaStare(Stare_usa_inchisa.Instance);
        }

        public override void Gateste(Context context)
        {
            // Nu se poate porni cu usa deschisa
        }

        public override void Tick_ceas(Context context)
        {
            // Nu face nimic
        }
    }
}