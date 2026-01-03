using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ControllerMicrounde
{
    public partial class MainWindow : Window, IAfisaj_microunde, IObserver<Stare>
    {
        private Context context;
        private IDisposable? unsubscriber;
        private bool timerRunning = true;

        public MainWindow()
        {
            InitializeComponent();
            context = Context.Instance;
            Subscribe(context);

            Set_usa_inchisa();
            Set_gateste_OFF();
            Set_timp_ramas(context.Timp_ramas);
            Set_microunde("Cuptorul este pregatit.");

            StartTicks();
        }

        public void Subscribe(IObservable<Stare> provider)
        {
            unsubscriber = provider.Subscribe(this);
        }

        public void Unsubscribe()
        {
            if (unsubscriber != null)
            {
                unsubscriber.Dispose();
            }
        }

        public void OnNext(Stare value)
        {
            if (value is Stare_usa_inchisa)
            {
                Set_usa_inchisa();
                Set_gateste_OFF();

                if (context.Timp_ramas == 10)
                {
                    Set_microunde("GATIREA S-A TERMINAT!");
                }
                else
                {
                    Set_microunde("Usa este inchisa");
                }
            }
            else if (value is Stare_usa_deschisa)
            {
                Set_usa_deschisa();
                Set_gateste_OFF();
                Set_microunde("Usa este deschisa");
            }
            else if (value is Stare_gateste_ON)
            {
                Set_usa_inchisa();
                Set_gateste_ON();
                Set_microunde($"Gateste...  {context.Timp_ramas} secunde ramase");
            }

            Set_timp_ramas(context.Timp_ramas);
        }

        public void OnError(Exception error)
        {
            Set_microunde($"Eroare: {error.Message}");
        }

        public void OnCompleted()
        {
            Set_microunde("Observarea s-a incheiat");
        }

        private async void StartTicks()
        {
            while (timerRunning)
            {
                await Task.Delay(1000);
                context.Tick_ceas();
            }
        }

        private void Deschide_usa_Click(object sender, RoutedEventArgs e)
        {
            context.Deschide_usa();
        }

        private void Inchide_usa_Click(object sender, RoutedEventArgs e)
        {
            context.Inchide_usa();
        }

        private void Gateste_ON_Click(object sender, RoutedEventArgs e)
        {
            if (context.Stare_curenta is Stare_usa_deschisa)
            {
                Set_microunde("EROARE: Inchideti usa inainte de a porni!");
                return;
            }
            context.Gateste();
        }

        public void Set_usa_deschisa()
        {
            labelUsaStatus.Content = "Usa deschisa! ";
            labelUsaStatus.Foreground = new SolidColorBrush(Colors.Orange);
        }

        public void Set_usa_inchisa()
        {
            labelUsaStatus.Content = "Usa inchisa! ";
            labelUsaStatus.Foreground = new SolidColorBrush(Colors.Green);
        }

        public void Set_gateste_ON()
        {
            labelGatireStatus.Content = "Gateste ON";
            labelGatireStatus.Foreground = new SolidColorBrush(Colors.Red);
            progressBarTimer.Foreground = new SolidColorBrush(Colors.Red);
        }

        public void Set_gateste_OFF()
        {
            labelGatireStatus.Content = "Gateste OFF";
            labelGatireStatus.Foreground = new SolidColorBrush(Colors.Gray);
            progressBarTimer.Foreground = new SolidColorBrush(Colors.Blue);
        }

        public void Set_timp_ramas(int timp)
        {
            labelTimer.Content = timp.ToString();
            progressBarTimer.Value = timp;
        }

        public void Set_microunde(string mesaj)
        {
            labelStatus.Content = mesaj;
        }

        protected override void OnClosed(EventArgs e)
        {
            timerRunning = false;
            Unsubscribe();
            base.OnClosed(e);
        }
    }
}