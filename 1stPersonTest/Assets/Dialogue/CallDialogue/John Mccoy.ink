INCLUDE ../globals.ink

Congratulations!<br>You must be the gopher.
+ [And you must be the real Mccoy.]
-> RealMccoy

=== RealMccoy ===
Bingo.
...
+ [Your name isn't really John Mccoy is it...]
-> RealName

=== RealName ===
What do you think?
+ [Okay, I'm already bored. Can I have the super secret intel?]
-> Intel

=== Intel ===
What's the hurry?@accessing remote terminal^
We only just met.
Why don't we get to know each other a little bit?@do not react^
What do you say?@say sure why not^
+ [Sure. Why not?]
-> SecretConversation

=== SecretConversation ===
Super! No reason we can't be friendly.@he is listening^
So how long have you been in the game?@but he cant see these messages^
+ [Too long if you ask me.]
-> DontTrust 

=== DontTrust ===
You and I both.@dont trust him^
No easy way out though.@he will betray u^
+ [That's what everyone keeps telling me.]
-> Info

=== Info ===
You should listen to them.
Anyway. I guess we better to do this.
Tell your boss...
"The chicken has flown the coop"
...
+ [That's it? That's all you have for me?]
-> BeInTouch

=== BeInTouch ===
Yep. That's it.@thats not it^
I better get going.@ill be in touch^

-> END