# Documentație Tehnică - ControllerMicrounde

## 1. Arhitectura generală a aplicației

### Descriere generală

**ControllerMicrounde** este o aplicație simulator pentru un cuptor cu microunde, dezvoltată în C# utilizând WPF (Windows Presentation Foundation). Aplicația implementează un sistem de control al stărilor cuptorului, permițând simularea comportamentului real al unui cuptor cu microunde prin interfața grafică.

Aplicația demonstrează utilizarea a trei design patterns fundamentale:
- **State Pattern** - pentru gestionarea stărilor cuptorului
- **Observer Pattern** - pentru notificarea schimbărilor de stare către interfața grafică
- **Singleton Pattern** - pentru asigurarea unei singure instanțe a contextului și stărilor

### Structura fișierelor proiectului

```
ControllerMicrounde/
├── ControllerMicrounde.sln           # Fișier soluție Visual Studio
└── ControllerMicrounde/
    ├── App.xaml                      # Configurare aplicație WPF
    ├── App.xaml.cs                   # Cod-behind pentru aplicație
    ├── AssemblyInfo.cs               # Informații assembly
    ├── ControllerMicrounde.csproj    # Fișier proiect C#
    ├── MainWindow.xaml               # Interfață grafică principală
    ├── MainWindow.xaml.cs            # Logică UI principală
    ├── Context.cs                    # Context/Stare curentă (Singleton + Observable)
    ├── Stare.cs                      # Clasă abstractă pentru stări
    ├── Stare_usa_inchisa.cs          # Stare concretă - ușă închisă
    ├── Stare_usa_deschisa.cs         # Stare concretă - ușă deschisă
    ├── Stare_gateste_ON.cs           # Stare concretă - gătire activă
    ├── Unsubscriber.cs               # Utilitar pentru dezabonare Observer
    └── IAfisaj_microunde.cs          # Interfață pentru afișaj
```

### Diagrama de clase

```
┌─────────────────────────────────────┐
│         <<abstract>>                │
│            Stare                    │
├─────────────────────────────────────┤
│ + Deschide_usa(Context)             │
│ + Inchide_usa(Context)              │
│ + Gateste(Context)                  │
│ + Tick_ceas(Context)                │
└─────────────────────────────────────┘
              △
              │
      ┌───────┴───────┬──────────────┐
      │               │              │
┌─────┴────────┐ ┌────┴────────┐ ┌──┴─────────────┐
│Stare_usa_    │ │Stare_usa_   │ │Stare_gateste_  │
│inchisa       │ │deschisa     │ │ON              │
├──────────────┤ ├─────────────┤ ├────────────────┤
│-instance     │ │-instance    │ │-instance       │
│+Instance     │ │+Instance    │ │+Instance       │
│<<Singleton>> │ │<<Singleton>>│ │<<Singleton>>   │
└──────────────┘ └─────────────┘ └────────────────┘
                        │
                        │
                ┌───────▼────────────────────────┐
                │        Context                 │
                ├────────────────────────────────┤
                │ -instance: Context             │
                │ -observers: List<IObserver>    │
                │ +Stare_curenta: Stare          │
                │ +Timp_ramas: int               │
                ├────────────────────────────────┤
                │ +Instance: Context             │
                │ +Subscribe(IObserver)          │
                │ +SchimbaStare(Stare)           │
                │ +Deschide_usa()                │
                │ +Inchide_usa()                 │
                │ +Gateste()                     │
                │ +Tick_ceas()                   │
                │ -NotifyObservers()             │
                │ <<Singleton>>                  │
                │ <<IObservable<Stare>>          │
                └────────────────────────────────┘
                        △
                        │ observă
                        │
        ┌───────────────┴─────────────────┐
        │         MainWindow              │
        ├─────────────────────────────────┤
        │ -context: Context               │
        │ -unsubscriber: IDisposable      │
        │ -timerRunning: bool             │
        ├─────────────────────────────────┤
        │ +OnNext(Stare)                  │
        │ +OnError(Exception)             │
        │ +OnCompleted()                  │
        │ +Set_usa_deschisa()             │
        │ +Set_usa_inchisa()              │
        │ +Set_gateste_ON()               │
        │ +Set_gateste_OFF()              │
        │ <<IObserver<Stare>>             │
        │ <<IAfisaj_microunde>>           │
        └─────────────────────────────────┘

┌──────────────────────────────┐
│    IAfisaj_microunde         │
├──────────────────────────────┤
│ + Set_usa_deschisa()         │
│ + Set_usa_inchisa()          │
│ + Set_gateste_ON()           │
│ + Set_gateste_OFF()          │
│ + Set_timp_ramas(int)        │
│ + Set_microunde(string)      │
└──────────────────────────────┘

┌──────────────────────────────┐
│      Unsubscriber            │
├──────────────────────────────┤
│ -observers: List<IObserver>  │
│ -observer: IObserver         │
├──────────────────────────────┤
│ +Dispose()                   │
│ <<IDisposable>>              │
└──────────────────────────────┘
```

### Diagrama de stări

```
                        ┌─────────────────────┐
                        │   Inițializare      │
                        └──────────┬──────────┘
                                   │
                                   ▼
                        ┌──────────────────────┐
              ┌────────►│  Stare_usa_inchisa   │◄────────┐
              │         └──────────┬───────────┘         │
              │                    │                     │
              │                    │ Gateste()           │
              │                    ▼                     │
              │         ┌──────────────────────┐         │
              │         │  Stare_gateste_ON    │         │
              │         └──────────┬───────────┘         │
              │                    │                     │
              │                    │ Tick_ceas()         │
              │                    │ (timp > 0)          │
              │                    │                     │
 Inchide_usa()│         Deschide_usa()        Tick_ceas()│
              │                    │           (timp = 0)│
              │                    ▼                     │
              │         ┌──────────────────────┐         │
              └─────────┤  Stare_usa_deschisa  │─────────┘
                        └──────────────────────┘

Tranziții:
──────────────────────────────────────────────────────────
Stare_usa_inchisa:
  • Deschide_usa() → Stare_usa_deschisa
  • Gateste() → Stare_gateste_ON (resetează timp)

Stare_usa_deschisa:
  • Inchide_usa() → Stare_usa_inchisa

Stare_gateste_ON:
  • Deschide_usa() → Stare_usa_deschisa
  • Gateste() → Stare_gateste_ON (adaugă timp)
  • Tick_ceas() → Stare_gateste_ON (timp > 0) sau
                  Stare_usa_inchisa (timp = 0)
```

---

## 2. Descrierea detaliată a claselor

### 2.1. Clasa `Stare` (abstractă)

**Fișier:** `Stare.cs`

**Rol:** Clasa abstractă de bază care definește interfața comună pentru toate stările cuptorului cu microunde. Implementează pattern-ul State, oferind un contract pentru toate operațiunile posibile ale cuptorului.

**Secvență de cod:**

```csharp
public abstract class Stare
{
    public abstract void Deschide_usa(Context context);
    public abstract void Inchide_usa(Context context);
    public abstract void Gateste(Context context);
    public abstract void Tick_ceas(Context context);
}
```

**Comportament:**
- Definește patru operațiuni abstracte care trebuie implementate de toate stările concrete
- `Deschide_usa()` - gestionează acțiunea de deschidere a ușii
- `Inchide_usa()` - gestionează acțiunea de închidere a ușii
- `Gateste()` - gestionează pornirea/continuarea procesului de gătire
- `Tick_ceas()` - gestionează trecerea timpului (apelat periodic)
- Fiecare metodă primește ca parametru instanța `Context` pentru a putea modifica starea globală

---

### 2.2. Clasa `Stare_usa_inchisa`

**Fișier:** `Stare_usa_inchisa.cs`

**Rol:** Reprezintă starea în care ușa cuptorului este închisă și nu se gătiește. Aceasta este starea implicită de start și starea finală după terminarea gătitului.

**Secvență de cod:**

```csharp
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
```

**Comportament:**
- **Singleton**: O singură instanță în aplicație, accesată prin proprietatea `Instance`
- **Deschide_usa()**: Tranzițiune către `Stare_usa_deschisa`
- **Inchide_usa()**: Nici o acțiune (ușa este deja închisă)
- **Gateste()**: Resetează timpul la valoarea inițială și tranzițiune către `Stare_gateste_ON`
- **Tick_ceas()**: Nici o acțiune (nu există numărătoare inversă când nu se gătiește)

---

### 2.3. Clasa `Stare_usa_deschisa`

**Fișier:** `Stare_usa_deschisa.cs`

**Rol:** Reprezintă starea în care ușa cuptorului este deschisă. În această stare, cuptorul nu poate începe gătitul.

**Secvență de cod:**

```csharp
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
```

**Comportament:**
- **Singleton**: O singură instanță accesată prin `Instance`
- **Deschide_usa()**: Nici o acțiune (ușa este deja deschisă)
- **Inchide_usa()**: Tranzițiune către `Stare_usa_inchisa`
- **Gateste()**: Nici o acțiune (măsură de siguranță - nu permite pornirea cu ușa deschisă)
- **Tick_ceas()**: Nici o acțiune

---

### 2.4. Clasa `Stare_gateste_ON`

**Fișier:** `Stare_gateste_ON.cs`

**Rol:** Reprezintă starea activă de gătire. În această stare, cuptorul funcționează și timpul scade cu fiecare secundă.

**Secvență de cod:**

```csharp
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
```

**Comportament:**
- **Singleton**: O singură instanță accesată prin `Instance`
- **Deschide_usa()**: Întrerupe gătitul și tranzițiune către `Stare_usa_deschisa`
- **Inchide_usa()**: Nici o acțiune (ușa este deja închisă când se gătiește)
- **Gateste()**: Adaugă timp suplimentar (10 secunde) și notifică observatorii
- **Tick_ceas()**: Decrementează timpul rămas; dacă ajunge la 0, resetează și tranzițiune către `Stare_usa_inchisa`; altfel rămâne în aceeași stare și notifică observatorii

---

### 2.5. Clasa `Context`

**Fișier:** `Context.cs`

**Rol:** Clasa centrală care menține starea curentă a cuptorului și coordonează comunicarea între stări și observatori. Implementează pattern-urile Singleton și Observable.

**Secvență de cod:**

```csharp
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
```

**Comportament:**
- **Singleton**: Asigură o singură instanță globală a contextului
- **Observable**: Implementează `IObservable<Stare>` pentru a notifica observatorii despre schimbările de stare
- **Stare_curenta**: Menține referința către starea curentă activă
- **Timp_ramas**: Numărul de secunde rămase pentru gătit
- **Subscribe()**: Permite observatorilor să se înregistreze pentru notificări
- **NotifyObservers()**: Notifică toți observatorii înregistrați despre schimbarea de stare
- **SchimbaStare()**: Schimbă starea curentă și notifică observatorii
- **Metode delegate**: `Deschide_usa()`, `Inchide_usa()`, `Gateste()`, `Tick_ceas()` - delegate apelurile către starea curentă

---

### 2.6. Clasa `MainWindow`

**Fișier:** `MainWindow.xaml.cs`

**Rol:** Fereastra principală a aplicației WPF care servește ca Observer pentru schimbările de stare și implementează interfața grafică a simulatorului de cuptor cu microunde.

**Secvențe de cod semnificative:**

```csharp
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

    private async void StartTicks()
    {
        while (timerRunning)
        {
            await Task.Delay(1000);
            context.Tick_ceas();
        }
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
}
```

**Comportament:**
- **Observer**: Implementează `IObserver<Stare>` pentru a primi notificări despre schimbările de stare
- **IAfisaj_microunde**: Implementează interfața pentru actualizarea elementelor UI
- **OnNext()**: Metodă callback care actualizează interfața grafică în funcție de noua stare
- **StartTicks()**: Creează un timer asincron care apelează `Tick_ceas()` la fiecare secundă
- **Event handlers**: Gestionează acțiunile utilizatorului (click butoane) și le transmite către Context
- **Validare**: Verifică dacă ușa este închisă înainte de a permite pornirea gătitului
- **Cleanup**: Dezabonează observer-ul la închiderea ferestrei pentru a preveni memory leaks

---

### 2.7. Clasa `Unsubscriber`

**Fișier:** `Unsubscriber.cs`

**Rol:** Clasă utilitar care implementează pattern-ul IDisposable pentru a permite dezabonarea observatorilor de la subiecte Observable.

**Secvență de cod:**

```csharp
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
```

**Comportament:**
- **IDisposable**: Implementează interfața pentru a permite curățarea resurselor
- **Constructor**: Primește lista de observatori și observatorul specific care trebuie eliminat
- **Dispose()**: Elimină observatorul din lista de observatori când este apelat
- **Utilizare**: Este returnat de metoda `Subscribe()` din Context și permite dezabonarea automată folosind pattern-ul `using` sau apelarea explicită

---

### 2.8. Interfața `IAfisaj_microunde`

**Fișier:** `IAfisaj_microunde.cs`

**Rol:** Definește contractul pentru interfața de afișaj a cuptorului cu microunde, separând logica de prezentare de logica de business.

**Secvență de cod:**

```csharp
public interface IAfisaj_microunde
{
    void Set_usa_deschisa();
    void Set_usa_inchisa();
    void Set_gateste_ON();
    void Set_gateste_OFF();
    void Set_timp_ramas(int timp);
    void Set_microunde(string mesaj);
}
```

**Comportament:**
- **Interfață**: Definește metodele necesare pentru actualizarea afișajului
- **Set_usa_deschisa()**: Actualizează UI-ul pentru a arăta că ușa este deschisă
- **Set_usa_inchisa()**: Actualizează UI-ul pentru a arăta că ușa este închisă
- **Set_gateste_ON()**: Actualizează UI-ul pentru a arăta că gătitul este activ
- **Set_gateste_OFF()**: Actualizează UI-ul pentru a arăta că gătitul este oprit
- **Set_timp_ramas()**: Afișează timpul rămas
- **Set_microunde()**: Afișează mesaje de stare generale
- **Separare concernelor**: Permite implementări multiple ale afișajului fără a modifica logica de business

---

## 3. Implementarea șablonului State

### Clasele implicate

Pattern-ul State este implementat prin următoarele clase:
- **`Stare`** - Clasa abstractă de bază (State)
- **`Stare_usa_inchisa`** - Stare concretă (ConcreteState)
- **`Stare_usa_deschisa`** - Stare concretă (ConcreteState)
- **`Stare_gateste_ON`** - Stare concretă (ConcreteState)
- **`Context`** - Contextul care menține starea curentă (Context)

### Modalitatea de utilizare

Pattern-ul State permite unui obiect să-și schimbe comportamentul atunci când starea sa internă se modifică. În aplicația noastră, comportamentul cuptorului se schimbă în funcție de starea curentă (ușă deschisă, ușă închisă, gătire activă).

**Exemplu de utilizare:**

```csharp
// Obținerea instanței Context (Singleton)
Context context = Context.Instance;

// Delegarea unei acțiuni către starea curentă
context.Deschide_usa();
// Intern, Context apelează:
// Stare_curenta.Deschide_usa(this);

// Starea curentă decide ce acțiune să întreprindă
// De exemplu, din Stare_usa_inchisa:
public override void Deschide_usa(Context context)
{
    context.SchimbaStare(Stare_usa_deschisa.Instance);
}

// Schimbarea stării și notificarea observatorilor
public void SchimbaStare(Stare stareNoua)
{
    Stare_curenta = stareNoua;
    NotifyObservers();
}
```

**Flux de execuție pentru pornirea gătitului:**

```csharp
// 1. Utilizatorul apasă butonul "Gateste"
private void Gateste_ON_Click(object sender, RoutedEventArgs e)
{
    context.Gateste();
}

// 2. Context delegă către starea curentă
public void Gateste()
{
    Stare_curenta.Gateste(this);
}

// 3. Stare_usa_inchisa procesează acțiunea
public override void Gateste(Context context)
{
    context.ResetTimp();                            // Resetează timpul
    context.SchimbaStare(Stare_gateste_ON.Instance); // Schimbă starea
}

// 4. SchimbaStare notifică observatorii
public void SchimbaStare(Stare stareNoua)
{
    Stare_curenta = stareNoua;
    NotifyObservers();  // UI-ul este actualizat automat
}
```

### Justificarea utilizării pattern-ului State

**Avantaje:**

1. **Separarea logicii de stare**: Fiecare stare are propria implementare a acțiunilor, evitând condiții `if-else` complexe în Context
2. **Extensibilitate**: Adăugarea de noi stări (ex: `Stare_dezghet`, `Stare_pauza`) nu necesită modificarea stărilor existente
3. **Principiul Open/Closed**: Clasele de stare sunt închise pentru modificare dar deschise pentru extindere
4. **Claritate**: Comportamentul specific fiecărei stări este evident și localizat
5. **Siguranță**: Tranziții invalide sunt prevenționate implicit (ex: nu poți porni gătitul cu ușa deschisă)

**Exemplu de complexitate evitată:**

Fără State pattern, Context ar conține:

```csharp
// COD PROBLEMATIC - fără State Pattern
public void Deschide_usa()
{
    if (stare == "usa_inchisa")
    {
        stare = "usa_deschisa";
    }
    else if (stare == "gateste_ON")
    {
        stare = "usa_deschisa";
    }
    // else ... multe alte condiții
}

public void Gateste()
{
    if (stare == "usa_inchisa")
    {
        ResetTimp();
        stare = "gateste_ON";
    }
    else if (stare == "usa_deschisa")
    {
        // Nu face nimic
    }
    else if (stare == "gateste_ON")
    {
        AdaugaTimp();
    }
}
```

Cu State pattern, această logică este distribuită în clase separate, fiecare responsabilă pentru propria sa stare.

---

## 4. Implementarea șablonului Observer

### Clasele implicate

Pattern-ul Observer este implementat folosind interfețele standard .NET și următoarele clase:

- **`IObservable<Stare>`** - Interfață standard .NET implementată de `Context`
- **`IObserver<Stare>`** - Interfață standard .NET implementată de `MainWindow`
- **`Context`** - Subiectul observabil (Subject/Observable)
- **`MainWindow`** - Observatorul (Observer)
- **`Unsubscriber`** - Clasă helper pentru dezabonare (implementează `IDisposable`)

### Fluxul de notificare

**1. Abonarea la notificări:**

```csharp
public MainWindow()
{
    InitializeComponent();
    context = Context.Instance;
    Subscribe(context);  // MainWindow se abonează la Context
    // ...
}

public void Subscribe(IObservable<Stare> provider)
{
    unsubscriber = provider.Subscribe(this);
}

// În Context:
public IDisposable Subscribe(IObserver<Stare> observer)
{
    if (!observers.Contains(observer))
    {
        observers.Add(observer);
    }
    return new Unsubscriber(observers, observer);
}
```

**2. Notificarea observatorilor:**

```csharp
// Când starea se schimbă, Context notifică toți observatorii
public void SchimbaStare(Stare stareNoua)
{
    Stare_curenta = stareNoua;
    NotifyObservers();
}

private void NotifyObservers()
{
    foreach (var observer in observers)
    {
        observer.OnNext(Stare_curenta);  // Apelează callback-ul observatorului
    }
}
```

**3. Procesarea notificărilor în Observer:**

```csharp
// MainWindow primește notificarea și actualizează UI-ul
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
```

**4. Dezabonarea (cleanup):**

```csharp
protected override void OnClosed(EventArgs e)
{
    timerRunning = false;
    Unsubscribe();  // Curăță resurse
    base.OnClosed(e);
}

public void Unsubscribe()
{
    if (unsubscriber != null)
    {
        unsubscriber.Dispose();  // Apelează Unsubscriber.Dispose()
    }
}

// În Unsubscriber:
public void Dispose()
{
    if (_observer != null && _observers.Contains(_observer))
    {
        _observers.Remove(_observer);  // Elimină din listă
    }
}
```

### Diagrama fluxului Observer

```
┌─────────────┐                    ┌──────────────┐
│   Context   │                    │  MainWindow  │
│  (Subject)  │                    │  (Observer)  │
└──────┬──────┘                    └──────┬───────┘
       │                                  │
       │    1. Subscribe(this)            │
       │◄─────────────────────────────────┤
       │                                  │
       │    2. Return Unsubscriber        │
       ├─────────────────────────────────►│
       │                                  │
       │                                  │
   [Starea se schimbă]                   │
       │                                  │
       │    3. OnNext(Stare_curenta)      │
       ├─────────────────────────────────►│
       │                                  │
       │                                  │
       │                             [Actualizează UI]
       │                                  │
```

### Implicațiile asupra implementării

**Avantaje:**

1. **Cuplare slabă**: Context nu cunoaște detalii despre MainWindow, doar că implementează `IObserver<Stare>`
2. **Reactivitate**: UI-ul se actualizează automat la orice schimbare de stare
3. **Extensibilitate**: Pot fi adăugați multipli observatori fără a modifica Context
4. **Consistență**: Toate componentele UI sunt sincronizate automat cu starea curentă
5. **Standard .NET**: Folosește interfețele `IObservable<T>` și `IObserver<T>` din framework

**Exemplu de extensibilitate:**

```csharp
// Se poate adăuga ușor un logger fără a modifica Context
public class MicrowaveLogger : IObserver<Stare>
{
    public void OnNext(Stare value)
    {
        Console.WriteLine($"[{DateTime.Now}] Stare: {value.GetType().Name}");
    }
    
    public void OnError(Exception error) { }
    public void OnCompleted() { }
}

// În aplicație:
Context context = Context.Instance;
var logger = new MicrowaveLogger();
context.Subscribe(logger);  // Logger-ul primește automat toate notificările
```

**Gestionarea resurselor:**

Pattern-ul `IDisposable` asigură că observatorii pot fi eliminați corect pentru a preveni memory leaks:

```csharp
// Folosire cu using statement
using (var subscription = context.Subscribe(observer))
{
    // Observer-ul primește notificări
} // La ieșirea din bloc, Dispose() este apelat automat
```

---

## 5. Implementarea șablonului Singleton

### Unde este utilizat

Pattern-ul Singleton este implementat în următoarele clase:
- **`Context`** - Asigură o singură instanță a contextului aplicației
- **`Stare_usa_inchisa`** - O singură instanță a stării
- **`Stare_usa_deschisa`** - O singură instanță a stării
- **`Stare_gateste_ON`** - O singură instanță a stării

### Justificarea utilizării

**Pentru Context:**
- Trebuie să existe o singură sursă de adevăr pentru starea curentă a cuptorului
- Multipli observatori trebuie să primească notificări de la aceeași instanță
- Configurația globală (timp inițial, timp rămas) trebuie să fie consistentă în toată aplicația

**Pentru clasele de Stare:**
- Stările sunt stateless (fără date specifice instanței)
- Nu există motiv să creăm multiple instanțe identice
- Economie de memorie - o singură instanță este reutilizată
- Compararea rapidă a stărilor folosind referințe (`value is Stare_usa_inchisa`)

### Exemple de cod

**Implementare Singleton în Context:**

```csharp
public class Context : IObservable<Stare>
{
    // Câmp static privat pentru instanța unică
    private static Context? instance = null;
    
    // Constructor privat - previne instanțierea externă
    private Context()
    {
        Timp_ramas = TIMP_INITIAL;
        Stare_curenta = Stare_usa_inchisa.Instance;
    }
    
    // Proprietate publică statică - punct de acces global
    public static Context Instance
    {
        get
        {
            // Lazy initialization - se creează doar la prima accesare
            if (instance == null)
            {
                instance = new Context();
            }
            return instance;
        }
    }
}
```

**Utilizare:**

```csharp
// Obținerea instanței - același obiect în toată aplicația
Context context1 = Context.Instance;
Context context2 = Context.Instance;

// context1 și context2 sunt aceeași instanță
Console.WriteLine(Object.ReferenceEquals(context1, context2)); // True
```

**Implementare Singleton în clasele de Stare:**

```csharp
public class Stare_usa_inchisa : Stare
{
    // Instanță statică privată
    private static Stare_usa_inchisa? instance = null;
    
    // Constructor privat
    private Stare_usa_inchisa() { }
    
    // Proprietate statică pentru acces
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
    
    // Metodele de stare
    public override void Deschide_usa(Context context)
    {
        // Folosește Singleton-ul altei stări
        context.SchimbaStare(Stare_usa_deschisa.Instance);
    }
}
```

**Utilizare în tranziții de stare:**

```csharp
// Toate tranziițiile folosesc aceleași instanțe Singleton
context.SchimbaStare(Stare_usa_inchisa.Instance);  // Întotdeauna aceeași instanță
context.SchimbaStare(Stare_usa_deschisa.Instance); // Întotdeauna aceeași instanță
context.SchimbaStare(Stare_gateste_ON.Instance);   // Întotdeauna aceeași instanță

// Verificarea tipului de stare folosind referințe
if (context.Stare_curenta is Stare_usa_deschisa)
{
    // Compararea se face eficient prin referință
}
```

### Avantaje și considerații

**Avantaje:**

1. **Control asupra instanței**: Garantează o singură instanță în toată aplicația
2. **Acces global**: Ușor de accesat din orice punct al aplicației
3. **Lazy initialization**: Instanța se creează doar când este necesară
4. **Economie de resurse**: Pentru stări (care sunt stateless), se reutilizează aceeași instanță
5. **Consistență**: Toți consumatorii lucrează cu aceeași stare

**Considerații:**

1. **Thread-safety**: Implementarea curentă nu este thread-safe; pentru aplicații multi-threaded, ar fi necesară sincronizare:

```csharp
// Implementare thread-safe (exemplu)
private static readonly object padlock = new object();

public static Context Instance
{
    get
    {
        lock (padlock)
        {
            if (instance == null)
            {
                instance = new Context();
            }
            return instance;
        }
    }
}
```

2. **Testare**: Singleton-urile pot complica testarea unitară; pentru testabilitate mai bună, s-ar putea folosi Dependency Injection

3. **Pattern combination**: În acest proiect, Singleton funcționează bine cu State și Observer pentru a crea o arhitectură coerentă

---

## 6. Concluzii

### Rezumatul design patterns utilizate

Aplicația **ControllerMicrounde** demonstrează utilizarea eficientă a trei design patterns clasice pentru crearea unui simulator robust și extensibil de cuptor cu microunde:

**1. State Pattern**
- **Scop**: Gestionează comportamentul variabil al cuptorului în funcție de starea sa
- **Implementare**: Clasa abstractă `Stare` cu trei implementări concrete
- **Beneficiu**: Eliminarea logicii condiționale complexe și separarea clară a comportamentelor

**2. Observer Pattern**
- **Scop**: Sincronizarea automată între logica de business și interfața grafică
- **Implementare**: `Context` ca Observable, `MainWindow` ca Observer
- **Beneficiu**: Cuplare slabă și actualizare automată a UI-ului

**3. Singleton Pattern**
- **Scop**: Asigurarea unei singure instanțe pentru Context și toate stările
- **Implementare**: Instanțe statice private cu constructori privați
- **Beneficiu**: Control centralizat și economie de resurse

### Beneficiile arhitecturii alese

**1. Modularitate și separarea concernelor**
- Logica de stare este separată de logica de afișare
- Fiecare clasă are o responsabilitate bine definită (Single Responsibility Principle)
- Interfața `IAfisaj_microunde` separat logica UI de implementarea specifică WPF

**2. Extensibilitate**
```csharp
// Adăugarea unei noi stări (ex: Dezghețare) este simplă:
public class Stare_dezghet : Stare
{
    private static Stare_dezghet? instance = null;
    public static Stare_dezghet Instance { get { /* ... */ } }
    
    // Implementare metodelor abstracte
}

// Adăugarea unui nou observer (ex: Logger):
public class Logger : IObserver<Stare>
{
    public void OnNext(Stare value) { /* log */ }
}
```

**3. Mentenabilitate**
- Codul este organizat logic și ușor de înțeles
- Modificările într-o stare nu afectează alte stări
- Bug-urile sunt izolate în clase specifice

**4. Testabilitate**
- Fiecare stare poate fi testată independent
- Comportamentul Observer poate fi verificat prin mock objects
- Logica de tranziție este clară și verificabilă

**5. Siguranță**
- Tranziții invalide sunt prevenționate la nivel de design
- Nu se poate porni gătitul cu ușa deschisă
- Timer-ul funcționează doar în starea de gătire

**6. Performanță**
- Singleton-urile reduc overhead-ul de memorie
- Notificările Observer sunt eficiente
- Nu există logică duplicată

### Schema de interacțiune completă

```
User Action (UI)
      │
      ▼
┌─────────────┐
│ MainWindow  │ (Observer)
│ Click Event │
└──────┬──────┘
       │
       │ Apelează metodă
       ▼
┌─────────────┐
│   Context   │ (Singleton + Observable)
│  Delegate   │
└──────┬──────┘
       │
       │ Delegă către starea curentă
       ▼
┌─────────────┐
│    Stare    │ (State Pattern + Singleton)
│  Concrete   │
└──────┬──────┘
       │
       │ Modifică Context
       ▼
┌─────────────┐
│   Context   │
│SchimbaStare │
└──────┬──────┘
       │
       │ Notifică
       ▼
┌─────────────┐
│ MainWindow  │ (Observer)
│   OnNext()  │
└──────┬──────┘
       │
       │ Actualizează
       ▼
     UI Update
```

### Lecții învățate

1. **Combinarea pattern-urilor**: State, Observer și Singleton lucrează împreună armonios
2. **Importanța separării concernelor**: Interfețele ajută la decuplarea componentelor
3. **Gestionarea resurselor**: `IDisposable` și `Unsubscriber` preveniți memory leaks
4. **Arhitectură reactivă**: Observer pattern face aplicația să reacționeze automat la schimbări

### Posibile îmbunătățiri viitoare

1. **Adăugare stări noi**: Dezghețare, Pregătire, Pauză
2. **Persistență**: Salvarea și restaurarea stării aplicației
3. **Configurare**: Parametri configurabili (timp inițial, putere microunde)
4. **Logging**: Sistem de logging pentru debugging și audit
5. **Thread-safety**: Implementare thread-safe a Singleton-urilor
6. **Dependency Injection**: Pentru testabilitate îmbunătățită
7. **Comenzi**: Command pattern pentru undo/redo
8. **Animații**: Tranziții animate între stări în UI

---

**Aplicația ControllerMicrounde este un exemplu educațional excelent de utilizare practică a design patterns-urilor GoF (Gang of Four) într-o aplicație WPF reală, demonstrând cum arhitectura bine gândită duce la cod mai curat, mai ușor de întreținut și mai extensibil.**
