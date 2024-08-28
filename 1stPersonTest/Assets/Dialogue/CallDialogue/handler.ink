INCLUDE ../globals.ink

{handlercallcount == 1: -> main}
{handlercallcount == 2: -> secondcall}
{handlercallcount == 3: -> thirdcall}
{handlercallcount == 4: -> fourthcall}

=== main ===
I don't like being kept waiting.
+ [How did you know it was me?]
-> HowDidYouKnowItWasMe
+ [You're lucky I called at all.]
-> YoureLuckyICalledAtAll
+ [I thought I was being followed. Had to circle the block.]
-> ThoughtIWasBeingFollowed

=== HowDidYouKnowItWasMe ===
Nevermind that. Here's what I need you to do.
+ [Straight to the point, I see. ]
-> Instructions

=== YoureLuckyICalledAtAll ===
As if you had a choice.
+ [Point taken. What do you want?]
-> Mission

=== ThoughtIWasBeingFollowed
Really? That's interesting.
+ ["Interesting" as in maybe I <i>was</i> being followed?]
-> Interesting

=== Interesting ===
No. I just didn't take you for the paranoid type.
+ [What type did you take me for?]
-> WhatType

=== WhatType ===
The type that knows when to do what they're told.
+ [Alright, I get the point. So what do you want this time?]
-> Mission

=== LessSpecial ===
You better pray that doesn't become the case...
Just kidding.
-> Instructions

=== IThoughtIWasYourIntermediary ===
No. You're more of what I'd call a "gopher".
-> Instructions

=== Instructions ===
Anyway. He passed along a number to contact him for the exchange.
+ [So why don't you call him yourself?]
-> CallHimYourself

=== Mission ===
I've been informed that one of my sources has uncovered some intel that may be relevant to my investigation.
+ [I thought <i>I</i> was your intermediary.]
-> IThoughtIWasYourIntermediary
+ [Informants of informants? Seems redundant.]
-> Redundant

=== Redundant ===
You should know by now that you can never have too many go-betweens in this line of work.
+ [Now I'm starting to feel redundant.]
-> LessSpecial

=== CallHimYourself ===
If only it were that simple.
Some of these guys are wound so tight, their conspiracies have conspiracies.
+ [That might be the understatement of the century.]
-> Understatement

=== Understatement ===
True. But can you really blame them?
+ [Good point...]
-> GuessNot

=== GuessNot ===
Anyway. Just call the number and you'll see for yourself.<br>Got a pen handy?<br>The number is 554-8923<br>Said his name was John. Didn't give a last name. Go figure.
+ [Got it.]
-> GotIt

=== GotIt ===
Good. Call me back with the details of the exchange.
~ handlercallcount = 2

-> END

=== secondcall ===
Did you get it?
+ [No. Haven't tried yet.]
-> HaventCalled

=== HaventCalled ===
Then what are you calling me for?
+ [Do you have any other information?]
-> MoreInformation

=== MoreInformation ===
I already told you.<br>His name is John...<br>No surname...<br>The number is 554-8923.
{handlercallcount == 3: -> DONE}
{handlercallcount == 4: -> DONE}
+ [That's not a lot to go on.]
-> NotAlotToGoOn

=== NotAlotToGoOn ===
For your sake, it better be...<br>And I'm not kidding this time.
->DONE

=== thirdcall ===
Tell me you have something.
+ [The number lead me to some kind of puzzle.]
-> SomeKindOfPuzzle

=== SomeKindOfPuzzle ===
Of course it did.
Well did you solve it?
+ [Not yet]
-> NotYet

=== NotYet ===
So you have nothing and you're calling me because...?
+ [Thought you might have some tips.]
-> SomeTips

=== SomeTips ===
My tip is...
Solve the goddamn puzzle and get me my intel.
+ [You really don't have anything else you can give me?]
-> MoreInformation

=== fourthcall ===
What have you got?
+ [It wasn't easy but I solved the puzzle.]
-> SolvedPuzzle

=== SolvedPuzzle ===
And?
+ [I got a number.]
-> GotANumber

=== GotANumber ===
And...?
+ [Looks like it's disconnected.]
-> NotYet

-> END 