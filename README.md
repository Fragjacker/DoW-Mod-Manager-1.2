![mod_manager_v15_thumbnail](https://user-images.githubusercontent.com/19975052/64079833-f833c000-ccec-11e9-96cb-ed5c01cc0791.png)
# DoW-Mod-Manager-1.53

This application allows for an easy launch of mods and management of large collections of mods for Warhammer 40K Dawn of War: Soulstorm:tm: and Warhammer 40K Dawn of War: Dark Crusade:tm:.
This version is supposed to fix some issues from version 1.4 of the Mod Manager while also adding some new functionality.

## INSTALLATION:

- In order to install the Mod Manager drop the "Dow Mod Manager Resources" folder and the "DoW Mod Manager v1.51.exe" into your primary game directory which is either:

  - "..\Dawn of War - Soulstorm\" or 
  - "..\Dawn of War - Dark Crusade\". 
  
  Do not put either Resources Folder nor the Executable file in a Subfolder or it won't work!

## MOD MANAGER USAGE:

- Once everything is in place launch the "DoW Mod Manager v1.51.exe" by double-clicking on it (You may want to create a shortcut of the Mod Manager on your Desktop
for convenience and faster access)
- Select a Mod from the left listing and it'll show if all necessary dependancy Mods are installed and if that is given, lets you directly launch the desired Mod
- You have the option to directly launch the unmodded vanilla Game by clicking the "START BASE GAME" button.

## MOD MANAGER FEATURE EXPLANATION:

- Install location: Shows the current location of the active Mod Manager executable. You can have multiple Soulstorm installations with their own Mod Manager 
(Simply repeat the installation process inside the new soulstorm install location)

- START BASE GAME Button: Does what the name implies, starts a new Sessien of the unmodded Soulstorm base game.

- START MOD: Starts the Game with the selected Advanced Start Options and the selected Mod if all dependancy mods are installed.

- TOGGLE LAA: This button allows for convenient activation and deactivation of the LAA flag for the Soulstorm.exe and GraphicsConfig.exe

- LAA Labels : These two labels will show if both the Soulstorm.exe and GraphicsConfig.exe are LAA patched. Since the UA Mod and many other mods push the game engine really hard towards it's limits
it's recommended to have the LAA(Large Address Awareness AKA. 4GB Patch) activated on both executables to reduce the chance for the game crashing when playing mods. Since most people use the LAA Patch in Online
matches anyways you'd do yourself a favor applying the LAA patch on said Executables. The LAA patch can ba applied and removed at any time by pressing the TOGGLE LAA button.

- Advanced Start Options:

  - dev : Starts the game in developers mode that provides helpful debug tools and enables execution of skripts (such as the autoexec.lua) in skirmish games, which are not available in the base game.
	(WARNING: Don't use this in multiplayer as it will not work.)
	
  - nomovies : Skips all movies and directly launches the game with the loading screen for a faster startup (Is checked by default for convenience)
 
  - forcehighpoly : This option forces the game to display the highes resolution LOD of a Model at any given distance. (Hint : This may have a negative impact on your performance, use with caution.)

## MOD MERGER:(WARNING: This is an experimental feature that requires some user responsibility, use with caution.)

- Pressing this button will open you another window where you have the opportunity to easily edit the Module file of any playable mod.
- To use it you have to select the Mod first that you want to edit from the dropdown list.
- Doing so will fill the upper Listbox with all the other Mods that your currently selected Mod will load upon launch.
The bottom list displays the Mods that are not currently loaded by your selected mod but are installed inside your soulstorm directory.
- You can select Mods from the bottom list and add them using the "+" Button. The selected Mod will be added to the top list and be set as "Active".
- Using the Green/Red arrows allows you to change the loadorder of the selected Mod by moving it Up/Down in the list.
- The Red "X" Button allows you to set a Mod to "Inactive" which means the the Mod will not be loaded into the Mod anymore but remain inside the Module file for later Reactivation if desired.
- With the "-" you can remove an "Active/Inactive" Mod from the list of loaded mods and move it back to the bottom list of available but not yet added mods.

HINT: None of the operations so far remove or edit anything of the selected Mod. Reselecting the currently loaded Mod or any other from the top dropdown list will discard your changes.

If you want to save your changes click the "SAVE" button. This will overwrite the currently selected core Mod with your new layout and generate a valid and launchable file.


With the Mod Manager you can easily Add or Remove ANY desired Mod you want. You can quickly create large combiner Mods with many race mods merged into another Mod.


For Example:

Selecting UltimateApocalypse_THB_DIDHT from the Dropdown list then adding any race Mod that you desire by selecting that race mod and hit "+" and then "SAVE" it, will load this particular race
into the Mod the next time you launch UltimateApocalypse_THB_DIDHT from the Mod Manager now.

To restore the previous layout (If you want to rollback) you can either Deactivate the newly added mods using "X", remove the Mod using "-" Button or create backup of UltimateApocalypse_THB_DIDHT.module file
somewhere on your computer for later restoration.

# Changelogs

## Version 1.53 (by IgorTheLight):

- Upgraded .NET Framework from 4.5 to 4.5.2 - a lot of small improvements. But also that will add "EnableWindowsFormsHighDpiAutoResizing" feature in app.config
- Enabled feature stated above
- Code refactoring - a lot of small tweaks here and there. More "C# friendly" variable names

## Version 1.52:

- Fixed an issue with the toggle LAA button to not generate proper checksums inside the executables. This rendered people unable to play online, since their executables were rejected.

## Version 1.51:

- Fixed an issue where the app would crash after quitting a game and clicking on the blank black space in the left available mods list.
- Fixed deselection of last started mod after firing up the game.

## Version 1.5:

Mod Merger changes:

- UI now scales properly with window size.
- Uses a dark themed color as well.

Mod Manager changes:

- Changed UI to use a dark color theme.
- UI now scales properly with window size.
- Added a button that allows for quick and easy toggle of the LAA flag on the relevant executables. (Integrates the functionality of the 4GB patch into one button)
- Added code that updates the mod manager entries as soon as some file gets changed/deleted in the file explorer, without having to restart the app.
- Added safeguards for missing art assets.
- Added persistant data for the last chosen mod and the checkbox options.
- Added support for Dark Crusade game as well.

## Version 1.4:

Mod Merger changes:

- Mod Merger was updated with additional logic to handle unexpected user inputs.
- The various Buttons will now only activate if they're needed and are able to provide actual functionality.
- New Disabled variants of the Button images added.
- Mod Merger allows now to overwrite an existing .module file with an updated one without crashing.
- A new Messagebox will tell you if saving the new module file was successfull.
- It's now possible to add/remove alot of Mods by just clicking the plus/minus Button. You won't have to reselect Mods you want to add/remove everytime.
- It's now possible to conveniently enable/disable mods as well without having to reselect them everytime.
- Added some Tooltips to the Mod Merger buttons to explain their functionality.

Mod Manager changes:

- The Mod manager will now only list mods that have the "Playable" flag set to 1.
- Added a new Button that allows for immediate start of the unmodded Base Soulstorm game.
- Added a new Label that will display if the Soulstorm.exe has the 4GB Patch (LAA Patch) applied or not.
- Added a new Label that will display if the GraphicsConfig.exe has the 4GB Patch (LAA Patch) applied or not.
- Mod Manager will now update it's mod list once you created a new Merged/altered an existing Mod with the Mod Merger.
