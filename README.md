# Gorkhon Script Editor
Alpha version of the script viewer/editor for Pathologic + Classic HD. This software allows a modder to disassemble the .bin compiled script files shipped with the IPL game Pathologic to study their functionality and to change it (to a limited extent, at this point).

## Importing a script

To begin working with a script, select File - Import from the top menu bar. The original binary is automatically backed up with the "gse_bak" file extension in the same folder as the script.

## Editing a script

Currently not recommended. If you're feeling brave and are familiar with the operand sizes for the game engine's assembly interpreter, you can directly edit operands in the main section of the app. Otherwise, I'd suggest waiting until a writeup for it is finished / asking on the Discord server.
After you're done with editing the operands of an instruction, click the "Reassemble instruction" (gear-like) button in the top bar and export the resulting script. 
