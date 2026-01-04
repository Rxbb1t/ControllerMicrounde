# Documentație Tehnică - ControllerMicrounde

## 1. Arhitectura aplicației

### Descriere generală

**ControllerMicrounde** este un simulator de cuptor cu microunde dezvoltat în C# cu WPF. Aplicația simulează comportamentul real al unui cuptor prin gestionarea stărilor acestuia.

Proiectul folosește trei design patterns:
- **State** - gestionarea stărilor cuptorului
- **Observer** - sincronizarea automată a interfeței grafice
- **Singleton** - instanțe unice pentru Context și stări

### Structura proiectului

```
ControllerMicrounde/
├── ControllerMicrounde.sln
└── ControllerMicrounde/
    ├── App.xaml & App.xaml.cs
    ├── MainWindow.xaml & MainWindow.xaml.cs
    ├── Context.cs                    # Singleton + Observable
    ├── Stare.cs                      # Clasă abstractă
    ├── Stare_usa_inchisa.cs
    ├── Stare_usa_deschisa.cs
    ├── Stare_gateste_ON.cs
    ├── Unsubscriber.cs
    └── IAfisaj_microunde.cs
```

### Diagrama de clase

```
              ┌──────────────┐
              │   Stare      │ (abstract)
              ├──────────────┤
              │ Deschide_usa │
              │ Inchide_usa  │
              │ Gateste      │
              │ Tick_ceas    │
              └──────┬───────┘
                     │
        ┌────────────┼────────────┐
        │            │            │
   ┌────┴───┐  ┌────┴────┐  ┌───┴─────┐
   │ Usa    │  │  Usa    │  │ Gateste │
   │Inchisa │  │Deschisa │  │   ON    │
   └────────┘  └─────────┘  └─────────┘
   (Singleton)  (Singleton)  (Singleton)

              ┌─────────────────┐
              │    Context      │ (Singleton)
              ├─────────────────┤
              │ Stare_curenta   │
              │ Timp_ramas      │
              │ observers       │
              ├─────────────────┤
              │ SchimbaStare()  │
              │ Deschide_usa()  │
              │ Inchide_usa()   │
              │ Gateste()       │
              └────────┬────────┘
                       │ (Observable)
                       ▼
              ┌─────────────────┐
              │   MainWindow    │ (Observer)
              ├─────────────────┤
              │ OnNext()        │
              │ Set_usa_*()     │
              │ Set_gateste_*() │
              └─────────────────┘
```

### Diagrama de stări

```
        ┌─────────────┐
        │ Inițializare│
        └──────┬──────┘
               ▼
        ┌─────────────┐
    ┌──►│ Usa Inchisa │◄───┐
    │   └──────┬──────┘    │
    │          │Gateste()  │
    │          ▼           │
    │   ┌─────────────┐    │
    │   │ Gateste ON  │    │
    │   └──────┬──────┘    │
    │          │Tick()     │
Inchide│   Deschide│    Tick()
    │          ▼      (timp=0)
    │   ┌─────────────┐    │
    └───│ Usa Deschisa│────┘
        └─────────────┘

Tranziții principale:
• Usa Inchisa → Gateste ON: pornire gătit
• Gateste ON → Usa Inchisa: timp expirat
• Orice stare → Usa Deschisa: deschidere ușă
• Usa Deschisa → Usa Inchisa: închidere ușă
```

---

## 2. Descrierea claselor

### 2.1. Clasa `Stare` (abstractă)

**Fișier:** `Stare.cs`

**Rol:** Definește interfața comună pentru toate stările cuptorului. Fiecare stare trebuie să implementeze cele patru metode abstracte.

```csharp
public abstract class Stare
{
    public abstract void Deschide_usa(Context context);
    public abstract void Inchide_usa(Context context);
    public abstract void Gateste(Context context);
    public abstract void Tick_ceas(Context context);
}
```

**Funcționalitate:**
- Fiecare metodă primește `Context` ca parametru pentru a putea schimba starea
- `Deschide_usa()` / `Inchide_usa()` - gestionează ușa
- `Gateste()` - pornește sau continuă gătitul
- `Tick_ceas()` - este apelat la fiecare secundă

---

### 2.2. Clasa `Stare_usa_inchisa`

**Fișier:** `Stare_usa_inchisa.cs`

**Rol:** Reprezintă starea implicită când ușa e închisă și cuptorul nu funcționează.

```csharp
public class Stare_usa_inchisa : Stare
{
    private static Stare_usa_inchisa? instance = null;

    public static Stare_usa_inchisa Instance
    {
        get
        {
            if (instance == null)
                instance = new Stare_usa_inchisa();
            return instance;
        }
    }

    public override void Deschide_usa(Context context)
    {
        context.SchimbaStare(Stare_usa_deschisa.Instance);
    }

    public override void Gateste(Context context)
    {
        context.ResetTimp();
        context.SchimbaStare(Stare_gateste_ON.Instance);
    }
}
```

**Comportament:**
- **Singleton** - o singură instanță în aplicație
- La `Deschide_usa()` → trece în `Stare_usa_deschisa`
- La `Gateste()` → resetează timpul și trece în `Stare_gateste_ON`
- `Inchide_usa()` și `Tick_ceas()` nu fac nimic (ușa e deja închisă)

---

### 2.3. Clasa `Stare_usa_deschisa`

**Fișier:** `Stare_usa_deschisa.cs`

**Rol:** Reprezintă starea când ușa este deschisă. Cuptorul nu poate funcționa în această stare.

```csharp
public override void Inchide_usa(Context context)
{
    context.SchimbaStare(Stare_usa_inchisa.Instance);
}

public override void Gateste(Context context)
{
    // Nu se poate porni cu usa deschisa - măsură de siguranță
}
```

**Comportament:**
- La `Inchide_usa()` → trece în `Stare_usa_inchisa`
- `Gateste()` nu face nimic (siguranță - nu pornește cu ușa deschisă)

---

### 2.4. Clasa `Stare_gateste_ON`

**Fișier:** `Stare_gateste_ON.cs`

**Rol:** Starea activă de gătire. Timpul scade cu fiecare secundă.

```csharp
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

public override void Gateste(Context context)
{
    context.AdaugaTimp();  // Adaugă 10 secunde
    context.SchimbaStare(this);
}
```

**Comportament:**
- `Tick_ceas()` decrementează timpul; la 0 oprește gătitul
- `Gateste()` adaugă timp suplimentar (10 secunde)
- `Deschide_usa()` întrerupe gătitul

---

### 2.5. Clasa `Context`

**Fișier:** `Context.cs`

**Rol:** Clasa centrală care ține starea curentă și coordonează comunicarea. Combină Singleton și Observable.

```csharp
public class Context : IObservable<Stare>
{
    private static Context? instance = null;
    public Stare Stare_curenta { get; private set; }
    public int Timp_ramas { get; set; }

    public static Context Instance
    {
        get
        {
            if (instance == null)
                instance = new Context();
            return instance;
        }
    }

    public void SchimbaStare(Stare stareNoua)
    {
        Stare_curenta = stareNoua;
        NotifyObservers();  // Anunță UI-ul
    }

    public void Deschide_usa()
    {
        Stare_curenta.Deschide_usa(this);
    }
}
```

**Funcționalitate:**
- **Singleton** - o singură instanță în aplicație
- **Observable** - notifică observatorii la schimbarea stării
- Delegă acțiunile către starea curentă
- Gestionează lista de observatori

---

### 2.6. Clasa `MainWindow`

**Fișier:** `MainWindow.xaml.cs`

**Rol:** Fereastra principală WPF care afișează starea cuptorului și reacționează la acțiunile utilizatorului.

```csharp
public partial class MainWindow : Window, 
                     IAfisaj_microunde, IObserver<Stare>
{
    public void OnNext(Stare value)
    {
        if (value is Stare_usa_inchisa)
        {
            Set_usa_inchisa();
            Set_gateste_OFF();
            Set_microunde(context.Timp_ramas == 10 
                ? "GATIREA S-A TERMINAT!" 
                : "Usa este inchisa");
        }
        else if (value is Stare_gateste_ON)
        {
            Set_gateste_ON();
            Set_microunde($"Gateste... {context.Timp_ramas}s");
        }
        // ...
    }

    private void Gateste_ON_Click(object sender, RoutedEventArgs e)
    {
        if (context.Stare_curenta is Stare_usa_deschisa)
        {
            Set_microunde("EROARE: Inchide usa!");
            return;
        }
        context.Gateste();
    }
}
```

**Funcționalitate:**
- Implementează `IObserver<Stare>` pentru a primi notificări
- `OnNext()` actualizează interfața când se schimbă starea
- Validează acțiunile (ex: nu pornește cu ușa deschisă)
- Timer asincron apelează `Tick_ceas()` la fiecare secundă

---

### 2.7. Clasa `Unsubscriber`

**Fișier:** `Unsubscriber.cs`

**Rol:** Permite dezabonarea observatorilor pentru a preveni memory leaks.

```csharp
public class Unsubscriber : IDisposable
{
    private List<IObserver<Stare>> _observers;
    private IObserver<Stare> _observer;

    public void Dispose()
    {
        if (_observer != null && _observers.Contains(_observer))
            _observers.Remove(_observer);
    }
}
```

**Utilizare:** Returnat de `Subscribe()` și permite cleanup automat.

---

### 2.8. Interfața `IAfisaj_microunde`

**Fișier:** `IAfisaj_microunde.cs`

**Rol:** Definește metodele pentru actualizarea afișajului.

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

**Scop:** Separă logica de prezentare de logica de business.
---

## 3. Pattern-ul State

### Cum funcționează

Pattern-ul State permite obiectului să-și schimbe comportamentul când se schimbă starea internă. În aplicația noastră, cuptorul se comportă diferit în funcție de stare.

**Clase implicate:**
- `Stare` - clasa abstractă
- `Stare_usa_inchisa`, `Stare_usa_deschisa`, `Stare_gateste_ON` - stări concrete
- `Context` - ține starea curentă

**Exemplu de utilizare:**

```csharp
// Click pe butonul Gateste
context.Gateste();
  → context.Stare_curenta.Gateste(this);
    → Stare_usa_inchisa schimbă în Stare_gateste_ON
      → Context notifică observatorii
        → UI-ul se actualizează automat
```

**Avantaje:**
- Evită cod cu multe `if-else`
- Fiecare stare gestionează propriul comportament
- Ușor de adăugat stări noi (ex: dezghețare, pauză)
- Tranziții invalide sunt prevenționate (nu pornește cu ușa deschisă)

---

## 4. Pattern-ul Observer

### Cum funcționează

Context (Observable) notifică automat MainWindow (Observer) când se schimbă starea.

**Flux:**
1. MainWindow se abonează la Context prin `Subscribe()`
2. Când se schimbă starea, Context apelează `NotifyObservers()`
3. Fiecare observer primește `OnNext(stare_noua)`
4. MainWindow actualizează interfața grafică

```csharp
// Context notifică observatorii
public void SchimbaStare(Stare stareNoua)
{
    Stare_curenta = stareNoua;
    foreach (var observer in observers)
        observer.OnNext(Stare_curenta);
}

// MainWindow primește notificarea
public void OnNext(Stare value)
{
    if (value is Stare_gateste_ON)
    {
        Set_gateste_ON();
        Set_microunde($"Gateste... {context.Timp_ramas}s");
    }
    // ...
}
```

**Avantaje:**
- UI-ul se actualizează automat la orice schimbare
- Pot fi adăugați mai mulți observatori (ex: logger, statistici)
- Context nu știe detalii despre observatori (cuplare slabă)
- Folosește interfețe standard .NET (`IObservable`, `IObserver`)

---

## 5. Pattern-ul Singleton

### Unde se folosește

Singleton asigură o singură instanță în toată aplicația:
- **Context** - o singură sursă pentru starea cuptorului
- **Stare_usa_inchisa**, **Stare_usa_deschisa**, **Stare_gateste_ON** - câte o instanță per stare

### Implementare

```csharp
public class Context
{
    private static Context? instance = null;
    
    private Context() { }  // Constructor privat
    
    public static Context Instance
    {
        get
        {
            if (instance == null)
                instance = new Context();
            return instance;
        }
    }
}

// Utilizare
Context context = Context.Instance;  // Mereu același obiect
```

**De ce Singleton?**
- **Context**: Toți observatorii trebuie să primească date de la aceeași sursă
- **Stări**: Nu au date proprii, deci o instanță per tip e suficientă
- Economisește memorie și permite comparații rapide prin referință

---

## 6. Concl uzii

### Rezumat

Proiectul demonstrează cum trei design patterns lucrează împreună:

**State** - gestionează comportamentul în funcție de stare  
**Observer** - sincronizează automat UI-ul cu starea  
**Singleton** - asigură consistența datelor

### Beneficii

- **Cod clar**: Fiecare clasă are un rol bine definit
- **Ușor de extins**: Noi stări sau observatori se adaugă simplu
- **Siguranță**: Tranziții invalide sunt blocate automat
- **Performanță**: Singleton-urile economisesc memorie

### Flux complet

```
User → MainWindow → Context → Stare → Context → MainWindow → UI
       (click)      (delegă)  (logic)  (notif)  (actualiz)
```

### Posibile îmbunătățiri

- Stări noi: Dezghețare, Pauză, Pregătire
- Logging pentru debugging
- Salvare/restaurare stare
- Thread-safety pentru Singleton-uri
- Parametri configurabili (timp, putere)

---

**ControllerMicrounde** este un exemplu practic de utilizare a design patterns GoF într-o aplicație WPF reală, demonstrând cum arhitectura bine gândită produce cod curat și ușor de întreținut.
