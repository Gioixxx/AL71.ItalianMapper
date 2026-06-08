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

## Primo avvio: scoperta VID/PID e mappa tasti

L'AL71 ha VID/PID ancora da rilevare. Apri lo **strumento diagnostico** nella UI,
premi i tasti sulla AL71 e registra `VID/PID` + `scanCode/vkCode`. Usa l'output per
completare `src/AL71.Layouts/Resources/italian.json` (mappa dei 71 tasti).

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
