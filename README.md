# MELHARFI 2D Game Engine 1.5
2D Game Engine + Network Library + Pathfinding + Windows GUI + Mouse Event handler "ALL-IN-ONE"

## *Description*

MELHARFI.dll is a standalone DLL to create 2D games easily or add an animated graphics to your application.
Don't worry about many details in the usual way or adding a complex third party application, only to make some graphics in your form.

It's commonly useful for beginners and for advanced people.

The library is built with C# so you can use it in any .NET language such as VB.NET since it supports .Net Framework 4.0 or higher and Visual Studio 2015 or higher (some lambda expression not handled by VS 2013 or older).

## *Tutorial*

See Wiki section

https://github.com/melharfi/MELHARFI-2D-Game-Engine/wiki


## *Features:*

*GDI+ with double buffering.

*encapsulating existing code to make it really easy:

*animation method:
 You can create animation, with parameters, with timer, a number of shapes, reversed or not, movement or stable...

*draw text, image, rectangle, Shapes (Polygone), animation.

*handle events* :
 MouseDoubleClic,
 MouseClic,
 MouseDown,
 MouseUp,
 MouseMove,
 MouseOut,
 MouseOver,
 
*AStar* algorithm to find the way path between point A to point B in a Map with obstacles to make a movement of a player.

*Network library to create an awesome Client / Server application for networking games

 Its LidgrenNetwork library inside my library, all thanks goes to him

 https://github.com/lidgren/lidgren-network-gen3

*Use encrypted assets to prevent from stooling your artwork from other people. There is a tool to encrypt and decrypt your assets

The library is divided into 3 pieces: 

**MELHARFI** : for all gaming/graphic representation stuff you need.

**MELHARFI.AStar** : for pathfinding

**MELHARFI.LidgrenNetwork** : for networking.

### Question 1
What is 2D Game Engine?

### Response 1
The 2D game engine is a library you can integrate into your existing project and lets you build games or graphics representations.
The engine already handles many details about initializing game parameters, screens, and events. You won't have to get bored with the gritty details and create a game in some few lines of code.

### Question 2
Why another 2D game engine while there are dozens of them?

### Response 2 :
In reality, I have a passion for creating games it was some couple of years.
I just wanted to create a game like you right now.

My favorite games were always 2D especially RPG/MMORPGs.
You'll soon see that there's not a lot of 2D MMORPG games (good ones), nor 2D games engines, maybe some old ones, but they are no longer supported, like SDL or XNA(reported to MONO by now).
You'll soon get stuck and no one can help you as i already try them and documentation is poor.
In the other hand, I was only familiar with C# language and the .NET technology, most of the engines was in C++.

### Question 3
What distinguishes it from the others 2D Game Engines

### Response 3:
create a game is not that hard using any library/engine you want with some peace of code given to help you to start, BUT the big problem I found is when I need to create some GUI (Graphic User Interface "controls") to interact with user, as a Chat Box, Text Input, Buttons, copy/paste / click ... i didn't find any clue

So you have to create all of them programmatically and design them and I know that create a game is easier than creating a controls itself because it needs to handle some advanced windows events like click/Keyboard on the shape and so many other stuff ... you know it's really complicated.

In fact, there are some GUI Engines shered but i found them ugly, or not in the same language (C++) or it needs special configuration, working only with DirectX or not free ... oh my god.

I just wanted to use the same controls that all we know, those of Windows, pretty good.

I soon get frustrated by that obstacle, and I found NO game engine that integrates some GUI inside.

All the engine use the DirectX technology to create games, and if you've done some try in programming with that technology you should know how it's hard to interact with the DirectX, so complicated FOR ME, so I wanted something simple,

Another problem was the interaction between object drawn and mouse handling Event because you should program it yourself and I didn't find it already supported in any engines.

Another problem was to integrate a network library to make my game multiplayer.

I looked on the internet and I found LidgrenNetwork and I just wondered if there's an engine with all that features

After some search I found NOTHING that I can appreciate, so here is the deal

-I need a 2D game engine writing in .Net technology

-I need it to be simple, not in DirectX.

-I need the ability to add the controls (GUI).

-I need a network library for my multiplayer game.

-I need an algorithm of path finding between point A until point B in a Map with obstacles.

-I need to crypt my assets to prevent others people from stole it or modify it

Searching google ...> nothing dud lol

What a good challenge if I write it myself and include all that in ALL-In-One and be Standlone !!
and job done for you, don't worry anymore ^^

### Question 4
Why GDI+? DirectX is more powerful that GDI+

### Response 4
Windows use GDI+ to draw some stuff on the Forms of its applications, it was not created for gaming purpose, even it's not supposed to support so many stuff on the same form, BUT nothing stops you from using it.

In fact, GDI+ is slower because it was not designed for such thing BUT only in the really old and slow computers like Pentium 3 or earlier with the old windows version, all of us use minimum a Core processor that is sufficient to handle it.

DirectX uses many technologies that let it pretty fast but if you are not a master of programming you can't achieve what you want.

Another thing is, that lib is not only made for gaming purpose, it's a way to animate your application or a representation.
Showing a tutorial in the first starting of your application for example, or a lot of stuff you can do like graphic representation ...

### Question 5
Does this library need me to know programming 

### Response 5
Yes, the library only cares about creating a context of gaming, parameters of the screen, double buffering and encapsulation of methods that let you draw things on the screen instead of the original methods not obvious.

There is a code to help you use the engine with all its modules (Network, Pathfinding, Graphics, Mouse Events ...).
I could add other stuff but my engine was created in the context of what my game need so far, (well I had stopped to continue my game because of less time i have) but i still continue make it more advanced.

Yes you need to know how to code with Net technology to achieve what you want but I am sure that it's not really hard.

Remember that there is a Wiki section to help you use this library : 
https://github.com/melharfi/MELHARFI-2D-Game-Engine/wiki

### Question 6
What is the disadvantage of MELHARFI 2D Game Engine ?

### Response 6

No special effect in animation like particle animation unless you add another library.
No rotation of image unless you create many instances of images in different situation and use it like animation by showing them one by one (purpose of Anim class)

It work only in windows form, no cross-platform "Linux / Mac"

I apologize if I say a mistake about something,
My code may look not optimized for some.

Licensed with MIT
http://choosealicense.com/licenses/mit

Feel free to contact me for any purpose or issue you find by my address email:

m.elharfi@gmail.com

If you like the project and would like to buy me a coffee, I’ll appreciate that

Paypal account : m.elharfi@gmail.com
