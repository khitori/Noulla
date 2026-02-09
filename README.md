Isn't complete yet...

---
<br>

## Folder structure:

- Program.fs --- EntryPoint
- MainWindow.axaml --- Declarative UI layout of main window presentation
- MainWindow.axaml.fs --- Functionality of main window
- /Assets/ --- Destination for sprites
  - Backgrounds/ --- Contains background images like: `Forest1.png`
  - Characters/ --- Contains character sprites like `Alice1.png`
- /Content/ --- Destination for a single .json file
  - Contains only one `story.json` file
- /Modules/ --- Destination for a core modules of engine like: `Parser.fs`

<br>

## `story.json` commands:
> Style: `command (argument1) (argument2) etc.`

> `"cmd"` is a main command operator

> Important to use only `.png` native images

> All parameters have `string` type

<br>
<br>


`background (img)` set the background image
> { "cmd": "*background*", "img": "*Forest1.png*" }

> `img` is a full file name in `Backgrounds` folder
<br>



`character_show (name) (position)` set the character sprite
> { "cmd": "*character_show*", "name": "*Alice1.png*", "position": "*left*" }

> `name` is a full file name in `Characters` folder

> `position` is a sprite position like: `left`, `center`, `right`
<br>

`character_clear (position)` clear character sprite
> { "cmd": "*character_clear*", "position": "*left*" }

> `position` same as in `character_show`
<br>

`title_show (text)` set title text
> { "cmd": "*title_show*", "text": "*something here...*" }
<br>

`title_clear` clear title text
> { "cmd": "*title_clear*" }
<br>

`text_show (text)` set main text
> { "cmd": "*text_show*", "text": "*something here...*" }
<br>

`text_clear` clear main text
> { "cmd": "*text_clear*" }
<br>

`choice_show (id) (text) (save) (next)` set the button with text and optionally save or goto function
> { "cmd": "*choice_show*", "id": "*1*", "save": "*name value*", "next": "*scene*" }

> `id` is a button identificator: 1 = left, 2 = center, 3 = right

> `save` variable for save file. example "sleep yes" will transcode into `string sleep = "yes"`

> `next` move to the scene in json that you need
<br>

`choice_hide (id)` hide button
> { "cmd": "*choice_hide*", "id": "*1*" }
<br>

`wait_click` wait to user click on main text surface
> { "cmd": "*wait_click*" }
<br>

`wait_choice` wait to user click on button
> { "cmd": "*wait_choice*" }
<br>

`goto (scene)` transition to the desired scene
> { "cmd": "*goto*" }

`save (data)` save variable to file
> { "cmd": "*save*", "data": "*name value*" }

> `data` same as `save` in `choice_show`
<br>

