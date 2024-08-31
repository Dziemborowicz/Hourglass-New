﻿# Hourglass

[![Latest build](https://github.com/i2van/hourglass/workflows/build/badge.svg)](https://github.com/i2van/hourglass/actions)
[![Latest release](https://img.shields.io/github/downloads/i2van/hourglass/total.svg)](https://github.com/i2van/hourglass/releases/latest)
[![License](https://img.shields.io/badge/license-MIT-yellow)](https://opensource.org/licenses/MIT)

This project is the modified [simple countdown timer for Windows](https://github.com/dziemborowicz/hourglass). The changes were made to the original **Hourglass** can be found [here](#hourglass-changes). The latest **Hourglass** installer or portable distribution can be downloaded [here](https://github.com/i2van/hourglass/releases/latest).

Visit the [original Hourglass site](https://chris.dziemborowicz.com/apps/hourglass) to learn more. The **Hourglass** FAQ can be found [here](https://github.com/i2van/hourglass/blob/develop/FAQ.md).

## Hourglass Command-line Example

```shell
hourglass -n on -a on -g on -c on -st on -w minimized -i left+title -t "Timer 1" 1h23
```

creates

- 1 hour 23 minutes long timer: `1h23`
- named **Timer 1**: `-t "Timer 1"`
- with the notification area icon: `-n on`
- with the timer window:
  - which is always on top: `-a on`
  - shows the reversed progress bar: `-g on`
  - displays the time in the digital clock format: `-c on`
  - shows the trigger time: `-st on`
  - initially minimized: `-w minimized`
  - has the time left and the timer name displayed in the title: `-i left+title`

Run `hourglass -h` to display the **Hourglass** [command-line reference](https://github.com/i2van/hourglass/blob/develop/Hourglass/Resources/Usage.txt) or select **Command-line usage** from the notification area context menu.

## Prerequisites

- [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48) (click [here](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net48-web-installer) to download the runtime Web Installer)

## Hourglass Changes

### Installer

- Adds the **Hourglass** executable path to the [Windows App Paths](https://learn.microsoft.com/en-us/windows/win32/shell/app-registration#using-the-app-paths-subkey), so the **Hourglass** [command-line](https://github.com/i2van/hourglass/blob/develop/Hourglass/Resources/Usage.txt) is available out of the box.

### Portable

#### Ngen Support

Processing the **Hourglass** with the [Native Image Generator (Ngen.exe)](https://learn.microsoft.com/en-us/dotnet/framework/tools/ngen-exe-native-image-generator) speeds up the **Hourglass** startup.

To run script as an **Administrator** press `Win`+`X` and select the **Windows PowerShell (Admin)** or **Command Prompt (Admin)**. Copy script full path to the console opened and execute one of the following:

- Generate the **Hourglass** native image and its dependencies and install in the native image cache:

```shell
ngen-Hourglass.bat install
```

- Delete the native images of the **Hourglass** and its dependencies from the native image cache:

```shell
ngen-Hourglass.bat uninstall
```

### Command-line

- New command `pause`
- New command `resume`
- New option `--digital-clock-time`, `-c`, `/c`
- New option `--multi-timers`, `-mt`, `/mt`
- New option `--activate-next`, `-an`, `/an`
- New option `--show-trigger-time`, `-st`, `/st`
- New option `--pause-after-loop-timer`, `-pl`, `/pl`
- Renamed option `--prompt-on-exit` to `--prompt-on-close`

See [command-line usage](https://github.com/i2van/hourglass/blob/develop/Hourglass/Resources/Usage.txt) for details.

### UI

- Uses a [Windows Task Dialog](https://learn.microsoft.com/en-us/windows/win32/controls/task-dialogs-overview) instead of a message box ([GitHub](https://github.com/kpreisser/TaskDialog)).
- The **Command-line usage** dialog can be accessed from the **About** dialog and vice versa.
- New **Accent** theme which uses [Windows accent color](https://www.google.com/search?q=Windows+accent+color). This theme is a default one.

### Notification Area

- The double click shows/hides all the timer windows.
- All the timers are arranged by the time left. The order of the timers is new, expired, paused, running.
- The red circle is shown over the notification area icon if at least one timer is expired.
- The pause sign is shown over the notification area icon if at least one timer is paused.
- The digits from `0` to `9` are the hotkeys for the timers in the notification area icon context menu.
- The **Pause all** menu command pauses all the running timers.
- The **Resume all** menu command resumes all the paused timers.
- The **Options** menu command opens the timer options menu. The shortcut is the holding the `Shift` during right click on the notification area icon.
- The **Silent mode** disables all the timers notifications. It is not preserved between runs.
- The **Exit** menu command prompts for closing all the running timer windows if the **Prompt on close** option is set.

### Timer Windows

#### Keyboard Shortcuts

- The `Esc` shortcut minimizes the timer window.
- The `F11` shortcut makes the timer window full screen and back.
- The `Ctrl`+`N` shortcut creates a new timer window.

#### Context Menu

- All the timer window commands are available in the timer window context menu.
- The shortcuts are displayed in the timer window context menu.
- The **Window title** submenu is available directly from the timer window context menu.
- The **Reset position and size** menu command sets the timer window default position and size.
- The **Restore**, **Minimize** and **Maximize** timer window commands are always present in the timer window context menu.
- The **Advanced options** / **Display time in the digital clock format** timer window context menu option toggles the displayed time digital clock time format. The command-line option is `--digital-clock-time`, `-c`, `/c`
- The **Advanced options** / **Show trigger time** timer window context menu option shows the trigger time in the timer window and in the notification area context menu. The command-line option is `--show-trigger-time`, `-st`, `/st`
- The **Advanced options** / **Activate next window when minimized or closed** timer window context menu option enables the next timer window activation when the current timer window is minimized or closed. The command-line option is `--activate-next`, `-an`, `/an`
- The **Pause all** timer window context menu command pauses all the running timers. Command-line command is `pause`
- The **Resume all** timer window context menu command resumes all the paused timers. Command-line command is `resume`
- The **Pause after each loop** timer window context menu command pauses the loop timer when it expires. Command-line command is `--pause-after-loop-timer`, `-pl`, `/pl`

#### Other

- All the timer windows are arranged by the time left. The order of the timer windows is new, expired, paused, running.
- The minimum timer window size is limited by the Windows only.
- The timer tooltip is shown if the timer window size is too small.
- The progress bar changes direction to vertical when the height is more than the width and vice versa.
- When the timer window is minimized or closed the next visible non-minimized timer window is activated.
- The time left is now used for editing instead of the original launch time.
- The timer window pops up on the current virtual desktop.
- The mouse double-click on progress border toggles full screen.
- The sound file can be placed to the `%LOCALAPPDATA%\Hourglass` or `%LOCALAPPDATA%\Hourglass\Sounds` directories. It can be the symbolic link created by the `mklink` system command.

### Misc

- The **Hourglass** is built deterministically using the [GitHub Actions](https://github.com/i2van/hourglass/actions).
