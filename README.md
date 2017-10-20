# Game Shield
This is a simple game guard which i made before for Conquer Online game to prevent the conquer.exe and magiceffect.ini files from being replaced  by other modified ones.

# How does it work?
The shield works on validating the hashs of the files and sending it to the server, validating the SHA256 hash on the server and sending back to the shield whether these files are valid or no. The Shield also blocks any other moded Conquer files from running while connected to the server. As long as you're connected to the server, you can still play as soon as the Shield is killed, You'll be kicked from the game itself.

# Improvements which can be made?

The shield all it does now is validate the files, but this can be taken to the next level by adding some protection techniques in it. You can also make use of the connection between the server and client to botjail invalid characters or even ban character by HWID which is sent with the client login. The shield can be used for any game and this is just a basic idea.

# Tested on which version? 

I tested it on a 5165 client and it seemed to work for but for some reason it was disconnecting after random time like (20 mins) for some players.

# LICENSE
This is released under MIT licence.
