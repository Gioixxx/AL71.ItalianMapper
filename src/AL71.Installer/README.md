# AL71.Installer (placeholder)

Pacchetto di installazione previsto per la **v1.0**. Non implementato nell'MVP.

Opzioni candidate:

- **WiX Toolset** (MSI) — installazione classica, gestione avvio/uninstall.
- **Inno Setup** — più semplice, script-based.
- **MSIX** — pacchetto moderno, ma con limiti sugli hook a basso livello/registro.

Requisiti dell'installer:

- Copiare i binari di `AL71.UI` (output `AL71.LayoutManager.exe`).
- Creare i collegamenti Start Menu.
- (Opzionale) registrare l'avvio automatico — già gestito a runtime dall'app via
  `HKCU\...\Run`, quindi non strettamente necessario nell'installer.
