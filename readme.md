# Persidera Time Tracker

A WinForms desktop time tracker for Windows, used internally at
**Persidera Industries LLC**. It is a fork of
[Amunak/TimeTracker](https://github.com/Amunak/TimeTracker) by
Jiří "Amunak" Barouš (MIT license), upgraded to **.NET 8** and extended
with billing periods and **Mercury invoicing** integration.

## Features

* Start/stop time tracking with a live elapsed-time display
* Set tracking categories (or pick one already in the table)
* Immediate totals (all rows / selection / selected category)
* Delete entries
* Open / Save time tracker table files (CSV-like `.timetracker` format — fully
  compatible with the original app)
* Generic window manager options (stay on top, show in notification area, ...)
* Language picker (Czech and English) with Windows locale autodetection
* **Configurable hourly rate** (default `$35/hr`)
* **Semi-monthly billing periods** that land on the 1st and 15th of each month
  (configurable), automatically shifted to the nearest weekday when those days
  fall on a weekend
* **Mercury invoicing**: create an accounts-receivable invoice in Mercury
  directly from the time tracked in the current billing period
* The Mercury API token is stored securely in **Windows Credential Manager**
  (never in plaintext settings)
* A status-bar readout of the current period, hours tracked and billable value

## Mercury invoicing

1. Open **Tools → Settings** and configure:
   * Hourly rate and billing days
   * Mercury Customer ID and Destination Account ID
   * Mercury API token (saved to Windows Credential Manager via the **Save Token** button)
2. Track some time.
3. Open **Tools → Create Invoice for Current Period**. The app calculates the
   current billing period, filters entries to it, and shows a confirmation
   dialog (`{hours}h @ ${rate}/hr = ${total}`).
4. On confirm, it calls the Mercury API to create the invoice.

Notes:

* Mercury invoicing requires an eligible subscription tier and a token with
  invoicing permissions. If the API returns **403 Forbidden**, the app fails
  gracefully with a clear message.
* The app runs fine with no Mercury configuration — the invoice command simply
  reports that it is "not configured".

## Building

Requires the .NET SDK (targets `net8.0-windows`).

```
dotnet build PersideraTimeTracker.sln -c Release
```

The executable is produced at
`PersideraTimeTracker/bin/Release/net8.0-windows/PersideraTimeTracker.exe`.

## Credits & license

Released under the MIT License. Original work
Copyright © 2017 Jiří "Amunak" Barouš
([Amunak/TimeTracker](https://github.com/Amunak/TimeTracker)).
Fork and Mercury invoicing additions Copyright © 2026
Persidera Industries LLC.
