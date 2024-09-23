# Hourglass Frequently Asked Questions

The original **Hourglass** FAQ can be found [here](https://chris.dziemborowicz.com/apps/hourglass/#faq).

- [How do I start, stop, pause or resume a timer?](#how-do-i-start-stop-pause-or-resume-a-timer)
- [What do symbols ⏺⏸⏹⏏🔁 mean?](#what-do-symbols--mean)
- [What formats are supported when entering a duration or date and time?](#what-formats-are-supported-when-entering-a-duration-or-date-and-time)
- [How do I start a second timer with the Hourglass?](#how-do-i-start-a-second-timer-with-the-hourglass)
- [How do I keep the timer window on top of other windows?](#how-do-i-keep-the-timer-window-on-top-of-other-windows)
- [How do I run a timer in full screen mode?](#how-do-i-run-a-timer-in-full-screen-mode)
- [How do I stop the timer from prompting me when I close the window?](#how-do-i-stop-the-timer-from-prompting-me-when-i-close-the-window)
- [How do I minimize the timer window to the notification area?](#how-do-i-minimize-the-timer-window-to-the-notification-area)
- [How do I repeat the timer automatically when it expires?](#how-do-i-repeat-the-timer-automatically-when-it-expires)
- [How do I stop the timer from popping up on top of other windows when it expires?](#how-do-i-stop-the-timer-from-popping-up-on-top-of-other-windows-when-it-expires)
- [How do I automatically close the timer window when the timer expires?](#how-do-i-automatically-close-the-timer-window-when-the-timer-expires)
- [How do I automatically shut down Windows when the timer expires?](#how-do-i-automatically-shut-down-windows-when-the-timer-expires)
- [How do I start a timer for a duration or until a date and time that I recently entered?](#how-do-i-start-a-timer-for-a-duration-or-until-a-date-and-time-that-i-recently-entered)
- [How do I clear recently entered durations and dates and times?](#how-do-i-clear-recently-entered-durations-and-dates-and-times)
- [How do I resume a timer that I accidentally closed?](#how-do-i-resume-a-timer-that-i-accidentally-closed)
- [How do I clear saved timers?](#how-do-i-clear-saved-timers)
- [How do I set a title for a timer?](#how-do-i-set-a-title-for-a-timer)
- [How do I change what is displayed in the timer window title?](#how-do-i-change-what-is-displayed-in-the-timer-window-title)
- [How do I change the timer window color theme?](#how-do-i-change-the-timer-window-color-theme)
- [Is there a dark color theme available?](#is-there-a-dark-color-theme-available)
- [How do I add a custom color theme?](#how-do-i-add-a-custom-color-theme)
- [How do I rename a custom color theme?](#how-do-i-rename-a-custom-color-theme)
- [How do I delete a custom color theme?](#how-do-i-delete-a-custom-color-theme)
- [How do I change the notification sound?](#how-do-i-change-the-notification-sound)
- [How do I add a custom notification sound?](#how-do-i-add-a-custom-notification-sound)
- [How do I remove a custom notification sound?](#how-do-i-remove-a-custom-notification-sound)
- [How do I loop the notification sound?](#how-do-i-loop-the-notification-sound)
- [How do I stop the timer keeping the computer from going to sleep?](#how-do-i-stop-the-timer-keeping-the-computer-from-going-to-sleep)
- [How do I show the time elapsed rather than the time left?](#how-do-i-show-the-time-elapsed-rather-than-the-time-left)
- [How do I create a shortcut that automatically starts a timer with the Hourglass?](#how-do-i-create-a-shortcut-that-automatically-starts-a-timer-with-the-hourglass)
- [What command-line arguments does the Hourglass accept?](#what-command-line-arguments-does-the-hourglass-accept)
- [How to speed up the Portable Hourglass startup?](#how-to-speed-up-the-portable-hourglass-startup)
- [What should I do if the Hourglass does not start?](#what-should-i-do-if-the-hourglass-does-not-start)
- [What should I do if there is no notification sound?](#what-should-i-do-if-there-is-no-notification-sound)

## How do I start, stop, pause or resume a timer?

To start a timer enter a duration or date and time in the large text box in the middle of the timer window and click **Start** or press `Enter`.

To stop a timer click **Stop** or press `Ctrl`+`S` or simply close the timer window.

To pause or resume a timer, click **Pause** or **Resume** or press `Ctrl`+`P`.

Note that the **Pause** and **Stop** buttons only appear when you move your mouse cursor onto the timer window.

## What do symbols ⏺⏸⏹⏏🔁 mean?

| Symbol | Meaning |
|:------:| ------- |
|   ⏺    | New     |
|   ⏸    | Paused  |
|   ⏹    | Stopped |
|   ⏏    | Expired |
|   🔁    | Looped  |

Taken from [here](https://en.wikipedia.org/wiki/Media_control_symbols#Symbols).

## What formats are supported when entering a duration or date and time?

**Minutes** - To start a timer for a specified number of minutes, enter the number of minutes:

- `1` - count down for 1 minute
- `5` - count down for 5 minutes
- `10` - count down for 10 minutes

**Specifying units** - To start a timer specifying the units, enter a number followed by one of the supported units: `seconds`, `minutes`, `hours`, `days`, `weeks`, `months` and `years` are supported:

- `30 seconds` - count down for 30 seconds
- `5 minutes` - count down for 5 minutes
- `7 hours` - count down for 7 hours
- `3 days` - count down for 3 days
- `25 weeks` - count down for 25 weeks
- `6 months` - count down for 6 months
- `2 years` - count down for 2 years

You can also use the short form of the supported units: `s`, `m`, `h`, `d`, `w`, `mo` and `y`:

- `30s` - count down for 30 seconds
- `5m` - count down for 5 minutes
- `7h` - count down for 7 hours
- `3d` - count down for 3 days
- `25w` - count down for 25 weeks
- `6mo` - count down for 6 months
- `2y` - count down for 2 years

**Combining units** - To start a timer using a combination of units, simply concatenate the units:

- `5 minutes 30 seconds` - count down for 5 minutes 30 seconds
- `5m30s` - count down for 5 minutes 30 seconds
- `7 hours 15 minutes` - count down for 7 hours 15 minutes
- `7h15m` - count down for 7 hours 15 minutes

**Decimal notation** - To start a timer specifying a duration that is not a full number, use decimal notation:

- `5.5 minutes` - count down for 5 minutes 30 seconds
- `1.5 hours` - count down for 1 hour 30 minutes
- `0.5 years` - count down for 6 months

**Short form duration** - To start a timer for a specified duration, you can alternatively use the short form `m:ss` or `h:mm:ss` formats:

- `5:30` - count down for 5 minutes 30 seconds
- `7:15:00` - count down for 7 hours 15 minutes

You can use `.` instead of `:` as the separator if you prefer:

- `5.30` - count down for 5 minutes 30 seconds
- `7.15.00` - count down for 7 hours 15 minutes

**Until time of day** - To start a timer until a specified time of day, enter the time of day in the `h am/pm`, `h:mm am/pm` or `h:mm:ss am/pm` formats:

- `2 pm` - count down until 2:00:00 pm
- `2:30 pm` - count down until 2:30:00 pm
- `2:30:15 pm` - count down until 2:30:15 pm

You can use `.` instead of `:` as the separator if you prefer:

- `2.30 pm` - count down until 2:30:00 pm
- `2.30.15 pm` - count down until 2:30:15 pm

If the specified time of day has already passed today, the timer will count down until the specified time of day tomorrow.

**Until date** - To start a timer until a specified date, enter the date in the `month day`, `day month`, `month day, year` or `day month year` formats:

- `January 1` - count down until midnight January 1
- `1 January` - count down until midnight January 1
- `January 1, 2019` - count down until midnight January 1, 2019
- `1 January, 2019` - count down until midnight January 1, 2019

You can use the full month name: `January`, `February`, `March`, `April`, `May`, `June`, `July`, `August`, `September`, `October`, `November`, `December`. Or you can use the short form of the month name: `Jan`, `Feb`, `Mar`, `Apr`, `May`, `Jun`, `Jul`, `Aug`, `Sep`, `Oct`, `Nov`, `Dec`:

- `Jan 1` - count down until midnight January 1
- `1 Jan` - count down until midnight January 1
- `Jan 1, 2019` - count down until midnight January 1, 2019
- `1 Jan, 2019` - count down until midnight January 1, 2019

Or you can use the month number instead or a two-digit year, if you prefer:

- `1/1` - count down until midnight January 1
- `01/01` - count down until midnight January 1
- `1/1/19` - count down until midnight January 1, 2019
- `01/01/19` - count down until midnight January 1, 2019
- `1/1/2019` - count down until midnight January 1, 2019
- `01/01/2019` - count down until midnight January 1, 2019

Note that the order of the day, month and year depends on your system settings in some cases.

**Until weekday** - To start a timer until a specified weekday, enter `Monday`, `Tuesday`, `Wednesday`, `Thursday`, `Friday`, `Saturday` or `Sunday`:

- `Monday` - count down until midnight Monday
- `Wednesday` - count down until midnight Wednesday
- `Saturday` - count down until midnight Saturday

You can also use the short form of the weekday: `Mon`, `Tue`, `Wed`, `Thu`, `Fri`, `Sat` or `Sun`:

- `Mon` - count down until midnight Monday
- `Wed` - count down until midnight Wednesday
- `Sat` - count down until midnight Saturday

To specify a day next week rather than this week, append next week. To specify the weekday after next, append next or after next:

- `Wednesday next week` - count down until midnight Wednesday next week
- `Wednesday next` - count down until midnight the Wednesday after next Wednesday
- `Wednesday after next` - count down until midnight the Wednesday after next Wednesday
- `Thu next week` - count down until midnight Thursday next week
- `Thu next` - count down until midnight the Thursday after next Thursday
- `Thu after next` - count down until midnight the Thursday after next Thursday

**Tomorrow** - To start a timer until the next day, enter `tomorrow`:

- `tomorrow` - count down until midnight tomorrow

**Combining date and time** - To start a timer until a specified date, specified weekday or tomorrow and specified time of day, join the date, weekday or tomorrow and the time of day and separate them with at or on as appropriate:

- `January 1, 2019 at 2 pm` - count down until 2 pm on January 1, 2019
- `2 pm on January 1, 2019` - count down until 2 pm on January 1, 2019
- `01/01/2019 at 2 pm` - count down until 2 pm on January 1, 2019
- `2 pm on 01/01/2019` - count down until 2 pm on January 1, 2019
- `Wednesday at 2 pm` - count down until 2 pm on Wednesday
- `2 pm on Wednesday` - count down until 2 pm on Wednesday
- `tomorrow at 2 pm` - count down until 2 pm tomorrow
- `2 pm tomorrow` - count down until 2 pm tomorrow

You can omit the `at` or `on` separating the date, weekday or tomorrow and the time of day in most cases:

- `January 1, 2019 2 pm` - count down until 2 pm on January 1, 2019
- `2 pm January 1, 2019` - count down until 2 pm on January 1, 2019
- `01/01/2019 2 pm` - count down until 2 pm on January 1, 2019
- `2 pm 01/01/2019` - count down until 2 pm on January 1, 2019
- `Wednesday 2 pm` - count down until 2 pm on Wednesday
- `2 pm Wednesday` - count down until 2 pm on Wednesday
- `tomorrow 2 pm` - count down until 2 pm tomorrow
- `2 pm tomorrow` - count down until 2 pm tomorrow

## How do I start a second timer with the Hourglass?

Right-click on any empty space in the timer window and select **New timer**.

## How do I keep the timer window on top of other windows?

Right-click on any empty space in the timer window and check **Always on top**.

## How do I run a timer in full screen mode?

To enter full-screen mode, press `Alt`+`Enter` or `F11`; double-click on any empty space in the timer window or right-click on any empty space in the **Hourglass** window and check **Full screen**.

To exit full-screen mode, press `Alt`+`Enter` or `F11` again; double-click on any empty space in the timer window or right-click on any empty space in the timer window and uncheck **Full screen**.

## How do I stop the timer from prompting me when I close the window?

Right-click on any empty space in the timer window and uncheck **Prompt on close**.

## How do I minimize the timer window to the notification area?

Right-click on any empty space in the timer window and check **Show in notification area**. Now when you minimize the timer window, it will be hidden in the notification area. To restore all the timer windows, double-click on the **Hourglass** icon in the notification area.

To remove the **Hourglass** icon from the notification area and restore any hidden timer windows, right-click on any empty space in the timer window and uncheck **Show in notification area**.

## How do I repeat the timer automatically when it expires?

Right-click on any empty space in the timer window and check **Loop timer**.

## How do I stop the timer from popping up on top of other windows when it expires?

Right-click on any empty space in the timer window and uncheck **Pop up when expired**.

## How do I automatically close the timer window when the timer expires?

Right-click on any empty space in the timer window and check **Close when expired**. Note that the timer window will not close until the notification sound has finished playing.

## How do I automatically shut down Windows when the timer expires?

Right-click on any empty space in the timer window and check **Shut down when expired** in the **Advanced options** submenu. Note that Windows will not shut down until the notification sound has finished playing.

## How do I start a timer for a duration or until a date and time that I recently entered?

When you relaunch the **Hourglass**, the last duration or date and time that you entered will be automatically populated in the input text field. To start a countdown for that duration or until that date and time, simply press `Enter`.

To start a timer for another duration or until another date and time that you recently entered, right-click on any empty space in the timer window and select the duration or date and time from the **Recent inputs** submenu.

## How do I clear recently entered durations and dates and times?

Right-click on any empty space in the timer window and select **Clear recent inputs** from the **Recent inputs** submenu.

## How do I resume a timer that I accidentally closed?

The **Hourglass** keeps track of running timers that you have closed. To resume a saved timer, right-click on any empty space in the timer window and select the timer from the **Saved timers** submenu.

You can also resume all saved timers by checking **Open all saved timers** in the **Saved timers** submenu.

And you can set the **Hourglass** to automatically open saved timers when it starts up by checking **Open saved timers on startup** in the **Advanced options** submenu.

## How do I clear saved timers?

Right-click on any empty space in the timer window and select **Clear saved timers** from the **Saved timers** submenu.

## How do I set a title for a timer?

Click in the text field that says **Click to enter title**, enter a title and press `Enter`.

To clear a title that you entered, click the title text field, delete the title text and press `Enter`.

## How do I change what is displayed in the timer window title?

By default, the timer window title displays the application name **Hourglass**. You can change it to display the time left, the time elapsed or the timer title instead.

Right-click on any empty space in the timer window and then select the appropriate option from the **Window title** submenu.

## How do I change the timer window color theme?

Right-click on any empty space in the timer window and select a color theme from the **Theme** submenu.

## Is there a dark color theme available?

Yes. Right-click on any empty space in the timer window and select **Dark theme** from the **Theme** submenu.

## How do I add a custom color theme?

Right-click on any empty space in the timer window, select **Manage themes...** from the **Theme** submenu. Pick a color theme from the drop-down menu to base the new theme on and click **New**.

Click on each color your want to change and select a color using the color picker. Type the theme name in the textbox at the top of the dialog and click **Save** to save your changes.

## How do I rename a custom color theme?

Right-click on any empty space in the timer window, select **Manage themes...** from the **Theme** submenu. Pick the color theme that you want to rename from the drop-down menu, type the theme name in the textbox at the top of the dialog and click **Save**.

The built-in color themes cannot be renamed.

## How do I delete a custom color theme?

Right-click on any empty space in the timer window, select **Manage themes...** from the **Theme** submenu. Pick the color theme that you want to delete from the drop-down menu and click **Delete**.

The built-in color themes cannot be deleted.

## How do I change the notification sound?

Right-click on any empty space in the timer window and select a sound from the **Sound** submenu.

## How do I add a custom notification sound?

Save the sound file to the folder where you installed the **Hourglass** (typically `%PROGRAMFILES(x86)%\Hourglass`) or to the `%LOCALAPPDATA%\Hourglass` directory or to the `Sounds` subdirectory.

Your sound file should then appear when you right-click on any empty space in the **Hourglass** window and select the **Sound** submenu.

The supported formats are `.aac`, `.m4a`, `.mid`, `.midi`, `.mp3`, `.wav` and `.wma`. If your sound file is not in one of the supported formats, you should convert it first.

## How do I remove a custom notification sound?

Delete the sound file from the folder where you installed the **Hourglass** (typically `%PROGRAMFILES(x86)%\Hourglass`). Be careful not to delete any other files.

## How do I loop the notification sound?

Right-click on any empty space in the timer window and check **Loop sound** from the **Sound** submenu.

## How do I stop the timer keeping the computer from going to sleep?

By default, the **Hourglass** keeps the computer from going to sleep so that progress can be displayed and the notification sound can be played.

To stop the **Hourglass** keeping the computer from going to sleep, right-click on any empty space in the timer window and check **Do not keep computer awake** in the **Advanced options** submenu.

If the computer goes to sleep while a timer is running, the **Hourglass** will try to wake the computer around the time the timer will expire so the notification sound can be played.

## How do I show the time elapsed rather than the time left?

Right-click on any empty space in the timer window and check **Show elapsed time instead of time left** in the **Advanced options** submenu.

## How do I create a shortcut that automatically starts a timer with the Hourglass?

To create a shortcut, right-click on your Desktop or the folder where you want to create the shortcut and select **Shortcut** from the **New** submenu. Enter the location where `Hourglass.exe` is (typically `%PROGRAMFILES(x86)%\Hourglass\Hourglass.exe`) followed by the duration or date and time for your countdown in quotes.

Then click **Next**, give your shortcut a name and click **Finish** to create the shortcut.

You can also specify additional [command-line](https://github.com/i2van/hourglass/blob/develop/Hourglass/Resources/Usage.txt) arguments to set other options for the timer.

## What command-line arguments does the Hourglass accept?

You can launch the **Hourglass** and immediately start a timer by running `Hourglass.exe "<duration or date and time>"`. For example, to launch the **Hourglass** and start a 5-minute timer, run `Hourglass.exe "5 minutes"`.

You can also set any option that you can set using the user interface by specifying additional command-line arguments. To view a full list of command-line arguments, run `Hourglass.exe --help` or click [here](https://github.com/i2van/hourglass/blob/develop/Hourglass/Resources/Usage.txt).

## How to speed up the Portable Hourglass startup?

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

## What should I do if the Hourglass does not start?

If the **Hourglass** does not start or fails silently, delete the **Hourglass** settings.

The **Hourglass** settings can be located by the following command (to run it press `Win`+`R` and copy-paste command below):

```shell
cmd /k dir "C:\Users\%USERNAME%\AppData\Local\Chris_Dziemborowicz*"
```

The settings are stored into the corresponding `hourglass.EXE` subdirectories.

The **Hourglass Portable** keeps settings next to the executable.

## What should I do if there is no notification sound?

Some audio files could not be played for Windows with no [Windows Media Player](https://en.wikipedia.org/wiki/Windows_Media_Player) installed.
To fix this, download the [NAudioHourglassPack](https://github.com/i2van/hourglass/releases/latest) and upzip its content to the **Hourglass** folder.

Audio files supported:

- `.aiff`
- `.mp3`
- `.wav`
