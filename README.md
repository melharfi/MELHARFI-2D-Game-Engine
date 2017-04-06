# MELHARFI 2D Game Engine 1.3
2D Game Engine + Network Library + Pathfinding + Windows GUI + Mouse Event handler "ALL-IN-ONE"

## *Description*

MELHARFI.dll is a DLL file that lets you create 2D games in a simple way or even an animated graphics on a form. you don't worry about many details in the usual way or add a complex third party application, only to make some dynamic graphics in your form.

It's commonly useful for beginners and for advanced people, as I’ll show you my own game that I build with it (at the end of that topic).

The library is built in C# language so you can use it in any .NET language such as VB.NET while it supports Framework 4.0 or higher and Visual Studio 2015 or higher (some lambda expression not handled by VS 2013 or older).

It's the first and unique library for graphics using GDI+ as far as I know.

## *Tutorial*

See Wiki section

https://github.com/melharfi/MELHARFI-2D-Game-Engine/wiki


## *Features:*

*GDI+ with double buffering.

*encapsulating existing code to make it really easy:

*animation method:
 You can create animation, with parameters, with timer, a number of shapes, reversed or not, movement or stable...

*draw text, image, rectangle, animation on the screen.

*handle events* :
 MouseDoubleClic,
 MouseClic,
 MouseDown,
 MouseUp,
 MouseMove,
 MouseOut,
 MouseOver,
 
*AStart* algorithm to find the way path between point A and point B in a Map with obstacles to make a movement of a player.


*Network library to create an awesome Client / Server application for networking games

 Its LidgrenNetwork library inside my library, all thanks goes to him

 https://github.com/lidgren/lidgren-network-gen3

*Use encrypted assets to prevent from stooling your artwork from other people. There is a tool to encrypt and decrypt your assets

The library is divided into 3 pieces: 

**MELHARFI** : for all gaming/graphic stuff you need.

**MELHARFI.AStar** : for pathfinding

**MELHARFI.LidgrenNetwork** : for networking.

### Question 1
What is 2D Game Engine?

### Response 1
The 2D game engine is a library that you can integrate into your existing project and lets you build games or graphics representations. The engine already handles many details about initializing game parameters, screens, and events. You won't have to get bored with the gritty details and then create a game in some few lines of code.

### Question 2
Why another 2D game engine while there are dozens of them?

### Response 2 :
In reality, I have a passion for creating games it was some couple of years. I just wanted to create a game like you right now.

my favorite games were always 2D especially RPG/MMORPGs. you'll soon see that there's not a lot of 2D MMORPG games (good ones), nor 2D games engines, maybe some old ones, but they are no longer supported, like SDL or XNA(reported to MONO by now). You'll soon get stuck and no one can help you. On the other hand, I was only familiar with C# language and the .NET technology, so most of all of the engine was in C++.

### Question 3
What distinguishes it from the others 2D Game Engines

### Response 3:
create a game is not that hard using any library/engine you want with some peace of code given to help you to start, BUT the big problem I found is when I need to create some GUI (Graphic User Interface) or simply the controls to interact with user, as a Chat Box, Text Input, Buttons, copy/paste / click i didn't find any clue

So you have to create all of them programmatically and design them and I know that create a game is easier than creating a controls itself because it needs to handle some windows events like click/Keyboard on the shape and so many other stuff ... you know it's really complicated.

In fact, there are some GUI Engines but it was ugly, or not in the same language (C++) or it needs special configuration, working only with DirectX or not free ... oh my god.

So I just wanted to use the same controls that all we know, those of Windows, pretty good and perfect.

I soon get frustrated by that obstacle, and I found NO game engine that integrates some GUI inside.

All the engine use the DirectX technology to create games, and if you've done some try in programming with that technology you should know how it's hard to interact with the DirectX, so complicated FOR ME, so I wanted something simple,

Another problem was the handling of Mouse Event that because you should program yourself and I didn't find it already supported in any engines, you know.

Another problem was to integrate a network library to make my game multiplayer.

I looked on the internet and I found LidgrenNetwork and I just wondered if there's an engine with all that features

After some search I found NOTHING that I can appreciate, so here is the deal

-I need a 2D game engine writing in .Net technology

-I need it to be simple, not in DirectX because it's so hard for me to code other stuff and a supported technology.

-I need the ability to add the controls (GUI)

-I need a network library for my multiplayer game

-I need an algorithm of path finding between point A until point B in a Map with obstacles.

-I need to crypt my assets to prevent others people from stole it or modify it

Searching google ...> nothing dud lol

What a good challenge if I write it myself and include all that in ALL-In-One stuff!!
and job done for you, don't worry anymore ^^

### Question 4
Why GDI+? DirectX is more powerful that GDI+

### Response 4
Windows use GDI+ to draw some stuff on the Forms of its applications, and it was not created for gaming purpose, even it's not supposed to support so many stuff on the same form, BUT nothing stops you from using it.

in fact, GDI+ is slower because it was not designed for such thing BUT only in the really old and slow computers like Pentium 3 or earlier with the old windows version, all of us use minimum a Core processor that is sufficient to handle it.

DirectX uses many technologies that let it pretty fast but if you are not a master of programming you can't achieve what you want.

Another thing is, that lib is not only made for gaming purpose, it's a way to animate your application or a representation.
Showing a tutorial in the first starting off your application for example, or a lot of stuff you can do.

### Question 5
Does this library need me to know programming 

### Response 5
Yes, the library only cares about creating a context of gaming, like initializing the GDI+, parameters of the screen, double buffering and encapsulation of method that let you draw thing on the screen instead of the original stuff a bit hard to figure out.

Make the network interaction, generating waypoint for a player, handling mouse event, I could add other stuff but my engine was created in the context of what my game needed until now, (well I had stopped to continue my game because of less time I have and other personal stuff)

so you had to know or learn.Net technology to finish your game but I am sure that it's not really hard to achieve what you need.

### Question 6
What is the disadvantage of MELHARFI 2D Game Engine ?

### Response 6
The first problem come about resizing the form, you would like to make your game a bit bigger or smaller ?, but such thing is not supported because of a pixel handling, you still can do it but all mouse event will not work.

The issue come when you resize a picture, its pixels inside becomes different than the original, you know degradation or scale ..., and when you use an event like mouse click on an image, it compare the pixel taken on the screen with one of the original formats of image, not the one stretched. So the condition will never be true.

No special effect in animation like particle animation unless you add another library. No rotation of image unless you create many instances of images in different situation and use it like animation by showing them one by one (purpose of Anim class)

It work only in windows form, no cross-platform

I apologize if I say a mistake about something,
My code may look not optimized for some.

Here is 2 links about one game I design to let you get an idea how is powerful, sorry if it's in French
https://www.youtube.com/watch?v=D6VQARWxfvs
https://www.youtube.com/watch?v=ufQObcknmVY

Licensed with MIT
http://choosealicense.com/licenses/mit

Feel free to contact me for any purpose or issue you find by my address email:

m.elharfi@gmail.com

If you like the project and would like to buy me a coffee, I’ll appreciate that

Paypal account : m.elharfi@gmail.com
