<<<<<<< HEAD
# MELHARFI-2D-Game-Engine
2D Game Engine All in one (Lidgren Network, AStar Pathfinding, Manager Engine)
=======
# MELHARFI 2D Game Engine
2D Game Engine + Network Library + Path finding + Windows GUI + Event handler "ALL-IN-ONE"

*Description*

MELHARFI.dll is a DLL file that make you create 2D games with a simple way or even an animated graphics on a form, so you don't worry about many details in the usual way or add a complex third party only to make some dynamic graphics in your form.

It's commonly useful for beginners like for advanced person as I’ll show you my own game that I build with it (in the end of that topic).

The library is built in C# language so you can use it in any .NET language as VB.NET while its support Framwork 4.0 or higher.

It's the first and unik library for graphics using GDI+ as far as i know.

*Tutorial*

See Wiki section

https://github.com/melharfi/MELHARFI-2D-Game-Engine/wiki


*Features:*

*GDI+ with double buffering.

*encapsulating existing code to make it really easy:

*animation method:
 You can create animation, with parameters, with timer, number of shapes, reversed or not, movement or stable...

*draw text, image, rectangle, animation on the screen.

*handle events :
 MouseDoubleClic,
 MouseClic,
 MouseDown,
 MouseUp,
 MouseMove,
 MouseOut,
 MouseOver,
 
*AStart* algorithm to find the way path between point A until point B in a Map with obstacles to make a movement of a player.


*Network library to create an awesome Client / Server application for networking games

 Its LidgrenNetwork library inside my library, all thanks goes to him

 https://github.com/lidgren/lidgren-network-gen3

*Use an encrypted assets to prevent from stooling your artwork from other people. There is a tool to encrypt and decrypt your assets

The library is divided by 3 pieces: 

**MELHARFI.Gfx** : for all gaming/graphic stuff you need.

**MELHARFI.AStar** : for pathfinding

**MELHARFI.LidgrenNetwork** for networking.

Question 1
What is 2D Game Engine?

Response 1
2D game engine is a library that you should integrate to your existing project and then let you build games or graphics representation, the engine already handle so many details about initializing game, parameters, screen, event ... without get bored with that details and then create a game in some few lines of code.

Question 2
Why another 2D game engine while there's dozens of them?

Response 2 :
In reality, I have a pation for creating games it was some couple of years, so i just wanted to create one just like you right now.

my favor games was always 2D especially RPG/MMORPG, in that case you'll soon admit that there's not a lot of 2D MMORPG games (good ones), nor for 2D games engines, maybe some old stuff no longer supported like SDL or XNA, and you'll soon get staked and no one help you, in the other hand i was only familiar with C# language in the .NET technology, so all most of the engine was in C++.

Question 3
What distinguish it from the others 2D Game Engines

Response 3:
create a scene, animation ... in a game is not that hard using any library/engine you want with some peace of code given to help you to start, BUT the big problem i found is when i need to create some GUI (Graphic User Interface) or simply the controls to interact with user, as a Chat Box, Text Input, Buttons, copy / past / click...

So you have to create all of them programmatically, and design them and i know that create a game is more easy than create a controls itself because it need to handle some windows events like click/Keyboard on the shape and so many other stuff ... you know it's really complicated.

In fact there's some GUI Engines but it was ugly, or not in the same language (C++) or it need special configuration, working only with DirectX or not free ... oh my god.

So I just wanted to use the same controls that all we know, those of Windows, pretty good and perfect.

i soon get frustrated by that obstacle, and i found NO game engine that integrate some GUI inside.

All the engine use the DirectX technology to create games, and if you've do some try in programing with that technology you should know how it's hard to interact with the DirectX, so complicated FOR ME, so i wanted something simple,

Other problem was the handling of Mouse Event that because you should program yourself and i didn't find it already supported in any engines, you know.

Another problem was to integrate a network library to make my game multiplayer.

i looked in the internet and i found that LidgrenNetwork, and i just wanted if there's an engine with all that features

After some search i found NOTHING that i can appreciate, so here is the deal

-I need a 2D game engine writing in .Net technology

-I need it to be simple, not in DirectX because it's so hard for me to code other stuff, and a supported technology.

-I need the ability to add the controls (GUI)

-I need a network library for my multiplayer game

-I need an algorithm of path finding between point A until point B in a Map with obstacles.

-I need to crypt my assets to prevent others people from stole it or modify it

Searching google ...> nothing dud lol

What a good challenge if i write it myself and include all that in ALL-In-One stuff!!
and job done for you, don't worry anymore ^^

Question 4
Why GDI+ ? DirectX is more powerful that GDI+

Response 4
Windows use GDI+ to draw some stuff on the Forms of its applications, and it was not created for gaming purpose, even it's not supposed to support so many stuff on the same form, BUT nothing stop you from using it.

in fact GDI+ is slower because it was not designed for such thing BUT only in the really old and slow computers like Pentium 3 or earlier with old windows version, all of us use minimum a Core processor that is sufficient to handle it.

DirectX use many technology that let it pretty fast but if you are not a master of programing you can't achieve what you want.

Another thing is, that lib is not only made for gaming purpose, it's a way to animate your application, or a representation.
Showing a tutorial in the first starting of your application for example, or a lot of stuff you can do.

Question 6
Did that library need me to know programing?

Response 6
Yes, the library only care about create a context of gaming, like initializing the GDI+, parameters of screen, double buffering and encapsulation of method that let you draw thing on the screen instead of the original stuff a bit hard to figure out.

Make the network interaction, generating waypoint for a player, handling mouse event, i could add other stuff but my engine was created in the context of what my game needed until now, (well i had stopped to continue my game because of less time i have and other personal stuff)

so you had to know or learn .Net technology to finish your game but i am sur that it's not really hard to achieve what you need.

Question 7
What is the disadvantage of MELHARFI

Response 7
First problem come about resizing the form, you would like to make your game a bit bigger or smaller, but such thing is not supported because of a pixel handling, you still can do it but all mouse event will not work.
the issue come when you resize a picture, it's pixel inside become different than the original, you know degradation or scale ..., and when you use an event like mouse click on an image, it compare the pixel taken on the screen with the one of the original format of image, not the one stretched. So the condition will never be true.

No special effect in animation like particle animation unless you add another library. No rotation of image unless you create many instance of images in different situation and use it like animation by showing them one by one (purpose of Anim class)

It work only in windows form, so no cross platform

I apologize if i say a mistake about something,
My code may look not optimized for some as i am not a pro of programming i had to say.
May only this help someone.

Here is 2 link about one game i design to let you get an idea how is powerful, sorry if it's in French
https://www.youtube.com/watch?v=D6VQARWxfvs
https://www.youtube.com/watch?v=ufQObcknmVY

License with MIT
http://choosealicense.com/licenses/mit

Feel free to contact me for any purpose or issue you find by my adresse email :

m.elharfi@gmail.com

If you like the project and would bay me a coffee I’ll appreciate that

Paypal account : m.elharfi@gmail.com
>>>>>>> fixing mouse event issues
