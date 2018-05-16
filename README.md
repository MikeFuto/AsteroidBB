# Asteroid
A recreation of the arcade classic "Asteroids" with C# and WPF.
----------------------------------------------------------------
This was the final project for a class on C# and GUI Development. There were several project options given, but I requested this project of recreating Asteroids and it got approved.

During the project, I learned a lot about keeping my code clean, modular, and easily extendible. 
- I learned how important optimization is for programs like this, and while my collision algorithm is still very slow, it is much better than it used to be. I plan on improving the algorithm by splitting up collision testing into different subsections (divide and conquer!).
- I also learned a lot about the roles that mathematics plays in games like this as I designed the physics behind the ship's movement (a basic thrust in zero-gravity system). 
- I learned that it was very hard for me to avoid "Magic Numbers" when dealing with setting up and altering timings and speeds for different game objects as they were rendered: This is one of my new "Areas of Improvement."
- I learned that I am better at working through bugs and fixing broken code if I am able to explain the code step-by-step to someone else (or to myself).
The code (XAML) behind my menus is currently lacking in its modularity and simplicity, but I hope to clean this up now that the project deadline has come and gone. I have already extended the game to include a rudimentary testing Battle Mode for two players that I hope to further flesh out with more battle arenas and 4-player support.

Thanks for checking out my project!

Copyright - Braden Boettcher
