# HyprScribe

HyprScribe is a lightweight, bloat-free application (source AND binary included currently < 300kb) designed to save typed or pasted text *NOW* without fumbling with save buttons, filenames, or save file dialogs.

## Philosophy

HyprScribe is not a word processor or a notes system.  Its a digital scratchpad.  It exists to preserve creative flow by saving text immediately and reliably, without requiring filenames, folders, or user decisions.

If you need structure or permanent storage, use another tool.

But if you need to just open an app and write short notes that are reliably saved to disk NOW without thinking, this is for you.

## Description

HyprScribe saves data from each tab on the `text changed` event to a guaranteed unique, randomly generated filename. NO save button required!  The minute you typed your first letter, it was already saved somewhere whether you knew it or not!  In the event of a power outage or system failure while typing, most of the user's text should already be written to the hard drive, if not all. This prevents good ideas from being lost or forgotten and allows for creative flow without having to fuss with a save button or deciding where to save the file at.

## Table of Contents

1. [Installation](#installation)
2. [Usage](#usage)
3. [General Usage Notes](#general-usage-notes)
3. [Features](#features)
4. [Contributing](#contributing)
5. [License](#license)
6. [Acknowledgments](#acknowledgments)
7. [Contact Information](#contact-information)

## Installation

HyprScribe is ONLY available for Linux operating systems. It requires Mono & MCS, SQLite, and GTK# 3.

To install HyprScribe, follow these steps:

### Install Dependencies

Arch Linux:  pacman -S mono sqlite gtk-sharp-3

Ubuntu:  sudo apt install mono-devel sqlite gtk-sharp3

### Install HyprScribe

1. Clone the repository:
git clone https://github.com/metalxxhead/hyprscribe.git 

2. Navigate to the project directory:
cd hyprscribe/ 

3. Build the project:
scripts/build.sh 

4. Run the application:
scripts/run.sh

5.  Optionally, set up a shortcut on your desktop to launch the program

## Usage

1. Open the app and start typing. No save button required!  The file will be automatically saved to the `user_data/current_tabs/` directory in plain text.  Use `Ctrl + Z` to Undo and `Ctrl + Shift + Z` to Redo.  As long as tabs are not closed, they will persist when the app is re-opened.  If no tabs exist from a previous session, a blank one will automatically be opened on program launch.

2. The active tab can be saved to an external plain text file through the menu option.

3. If the user closes the tab, the corresponding tab will be removed, and the plain text file will be moved to the `archived_tabs` directory.  It's like ripping a page off your scratch pad:  You can still get it back, but its not on your scratch pad anymore.

4. The user must manually delete their data from these folders.  HyprScribe never deletes anything, leaving it to the user to safely purge their documents when they have time, whenever and however they see fit.

5. The `tabs.db` file in the `user_data` directory contains no user data; it only stores:
- Tab data i.e. tab label text (Tab 0, Tab 1, etc)
- The zero-based tab index (integer) for each tab
- and the file path using a randomly generated filename.

If you delete `tabs.db` while files still exist in the `current_tabs` directory, those tabs will not work anymore and you will need to manually remove your files from the `current_tabs` directory.

## General Usage Notes

HyprScribe is NOT a word processor OR a place to store your notes.  Its job is to save your ideas and other important text data NOW without having to fumble with save buttons or save file dialogs.  Just type and go.  If you need a fresh slate but you have already typed something, just open a new tab!  ONLY close tabs when you want to permanently archive them.  You can still get the data back though, but it will be in the `archived_tabs` folder where you'll eventually need to manually delete it anyway.

In most cases, if you don't care about anything you've typed or have already saved it elsewhere, just delete the `user_data` folder and restart the program to start fresh!

It is recommended to start over fresh as often as possible.  The cleaner you keep the `archived_tabs` directory, the easier it will be to find archived files using your system file manager or command line tools like `grep`.  No user data is ever saved anywhere else other than these folders.  The `user_data` folder and its subfolders, as well as `tabs.db` are automatically regenerated when deleted, but the program does not re-import files placed in the `current_tabs` directory or reload them into the `tabs.db` file after it has been deleted.

If you do this incorrectly, it might temporarily mess things up but is easily fixable.  Just save your data first from both the `current_tabs` and `archived_tabs` folders, as well as `tabs.db`, or just delete the entire `user_data` folder altogether.  The program will start fresh with no data and the folder structure and db file will be regenerated just like they are on a fresh install.

## Features

- Automatically saves text data on the `text changed` event of the text view
- Generates unique, randomly named filenames for each saved file
- Minimizes data loss in case of power outages or unexpected shutdowns
- Provides a menu option to export the active tab if desired
- Moves closed tabs to the `archived_tabs` directory
- Requires manual deletion of user data


## Contributing

Contributions are welcome! Please submit bug reports, feature requests, or pull requests.

## License

HyprScribe - A quick note taking app
Copyright (C) 2026  @metalxxhead

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.

## Acknowledgments

- [Mono](https://www.mono-project.com/)
- [MCS](https://www.mono-project.com/docs/advanced/runtime/mcs/)
- [SQLite](https://www.sqlite.org/)
- [GTK# 3](https://www.mono-project.com/docs/gui/gtksharp/)

## Contact Information

Damien's website: [internalstaticvoid.dev](https://internalstaticvoid.dev)

Damien's Github:  [github.com/metalxxhead](https://github.com/metalxxhead)


