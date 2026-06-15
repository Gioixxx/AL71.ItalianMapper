# AL71 Italian Layout Manager

Applicazione Windows per la tastiera **YUNZII AL71** che applica un layout italiano
software, consente il remap per-tasto su layer multipli (Normale / Shift / AltGr /
Shift+AltGr / Fn), gestisce profili e macro, parte con Windows e vive nella tray.

> **MVP** — questo primo rilascio contiene lo scaffold completo della solution e il
> **motore di remapping** (hook globale `WH_KEYBOARD_LL`), il layout italiano,
> il salvataggio JSON, la tray icon e l'avvio automatico.

## ⚠️ Prima di iniziare: valuta VIA/QMK

L'AL71 con ogni probabilità supporta il remapping **in firmware** tramite
[VIA](https://usevia.app) (scarica il JSON del tuo modello dalla
[pagina software YUNZII](https://www.yunzii.com/pages/software)). Quel percorso è
gratuito, senza lag e segue la tastiera su qualsiasi PC. Questa app software ha senso
per layer/macro custom oltre il firmware e per un layout italiano software a prescindere
dalla tastiera.

## Requisiti

- Windows 10/11
- .NET 8 SDK
- Visual Studio 2022 (o `dotnet` CLI)

> ⚠️ Il progetto è **Windows-only** (WPF, hook Win32, HID, registro). Non si compila né
> si esegue su Linux/macOS.

## Build

```pwsh
dotnet build AL71.LayoutManager.sln -c Release
dotnet run --project src/AL71.UI
```

## Test

La logica del motore di remap (selezione layer, AltGr, soppressione, scorciatoie) è
coperta da test xUnit in `src/AL71.Tests`, eseguiti anche in CI:

```pwsh
dotnet test AL71.LayoutManager.sln -c Release
```

## Architettura

| Progetto            | Responsabilità |
|---------------------|----------------|
| `AL71.Core`         | Modelli di dominio e interfacce (nessuna dipendenza Win32) |
| `AL71.Hook`         | Hook tastiera a basso livello, motore di remap, iniezione tasti |
| `AL71.Device`       | Rilevamento connessione/disconnessione AL71 (HID + WMI) |
| `AL71.Layouts`      | Definizione layout italiano (JSON) |
| `AL71.Persistence`  | Salvataggio/caricamento profili, backup, logging (Serilog) |
| `AL71.UI`           | App WPF/MVVM: dashboard, tastiera visuale, editor, tray, autostart |
| `AL71.Installer`    | Placeholder per l'installer (v1.0) |

Il motore di remap è **disaccoppiato dalla UI** e gira su un thread dedicato con
message loop, come richiesto dagli hook a basso livello.

## Primo avvio: procedura guidata (VID/PID + mappa tasti)

> Poiché l'AL71 **non è compatibile VIA/QMK**, VID/PID e mappa tasti vanno ricavati
> dall'app stessa.

Al primo avvio (o da **Diagnostica → Avvia procedura guidata AL71**) parte una
**procedura guidata** in 4 passi:

1. **Benvenuto** — istruzioni (cavo USB, interruttore su WIN).
2. **Identifica la tastiera** — metodo *scollega/ricollega*: "Fotografa" i dispositivi
   HID, scollega l'AL71, "Rileva" → l'app capisce quale è sparita e ne ricava VID/PID
   (in alternativa, selezione manuale dall'elenco).
3. **Cattura mappa tasti** — premi i tasti evidenziati sulla tastiera visuale; l'app
   registra `scanCode`/`vkCode` di ciascuno (il remap è sospeso durante la cattura).
4. **Riepilogo e salvataggio** — scrive `device.json` e `keymap-al71.json` in
   `%AppData%\AL71LayoutManager\Diagnostics` e imposta l'AL71 come tastiera target.

Usa `keymap-al71.json` per completare/affinare
`src/AL71.Layouts/Resources/italian.json` (mappa dei 71 tasti).

## Gestione profili

Dalla scheda **Dashboard** puoi gestire più profili (Italiano, Gaming, ...):

- **Nuovo** — crea un profilo vuoto e lo attiva.
- **Duplica** — copia il profilo attivo con un nuovo nome.
- **Rinomina** / **Elimina** — gestione del profilo attivo (non puoi eliminare l'unico rimasto).
- **Importa… / Esporta…** — scambia profili come file JSON (utile per backup o condivisione).
- **Ripristina IT** — riporta le mappature del profilo attivo al layout italiano di base.
- **Descrizione** — nota libera per profilo, modificabile e salvabile dalla Dashboard.

I nomi duplicati vengono resi univoci automaticamente (suffisso `(2)`, `(3)`, ...).
La Dashboard mostra anche il **conteggio dei tasti mappati**; nella scheda **Tastiera**
i tasti rimappati sono evidenziati in ambra.

## Configurazione

Dati utente in `%AppData%\AL71LayoutManager\`:

```
Profiles/   Settings/   Logs/   Backups/
```

## Limiti noti (MVP)

- L'hook globale `WH_KEYBOARD_LL` **non distingue la tastiera sorgente**: il remap si
  applica a tutte le tastiere mentre l'AL71 è connessa. L'isolamento reale per-dispositivo
  richiede il driver [Interception](https://github.com/oblitum/Interception) (roadmap v2.0).

## Roadmap

- **v1.0** — UI completa, editor visuale, profili, import/export
- **v2.0** — macro, layer Fn/Mouse/Unicode, hotkey profili, driver Interception
- **v3.0** — supporto altre tastiere ANSI, marketplace layout, plugin
