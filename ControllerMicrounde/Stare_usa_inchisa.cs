using System;

namespace ControllerMicrounde
{
    public class Stare_usa_inchisa : Stare
    {
        private static Stare_usa_inchisa? instance = null;

        private Stare_usa_inchisa() { }

        public static Stare_usa_inchisa Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Stare_usa_inchisa();
                }
                return instance;
            }
        }

        public override void Deschide_usa(Context context)
        {
            context.SchimbaStare(Stare_usa_deschisa.Instance);
        }

        public override void Inchide_usa(Context context)
        {
            // Usa este deja inchisa
        }

        public override void Gateste(Context context)
        {
            context.ResetTimp();
            context.SchimbaStare(Stare_gateste_ON.Instance);
        }

        public override void Tick_ceas(Context context)
        {
            // Nu face nimic
        }
    }
}