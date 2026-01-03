using System;

namespace ControllerMicrounde
{
    public class Stare_gateste_ON : Stare
    {
        private static Stare_gateste_ON? instance = null;

        private Stare_gateste_ON() { }

        public static Stare_gateste_ON Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Stare_gateste_ON();
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
            // Usa este deja inchisa cand gateste
        }

        public override void Gateste(Context context)
        {
            context.AdaugaTimp();
            context.SchimbaStare(this);
        }

        public override void Tick_ceas(Context context)
        {
            context.Timp_ramas--;

            if (context.Timp_ramas <= 0)
            {
                context.ResetTimp();
                context.SchimbaStare(Stare_usa_inchisa.Instance);
            }
            else
            {
                context.SchimbaStare(this);
            }
        }
    }
}