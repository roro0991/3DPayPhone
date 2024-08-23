INCLUDE ../globals.ink

{testcall_Count == 0: -> main}

=== main ===
Hello there my friend.@this is a secret message^
This is a test.
-> Extention
=== Extention ===
~SetExtentionSystem(true)
If you know the extention you're trying to reach please dial it now followed by \#.
~SetExtentionSystem(false)
{extention == "444": -> SallyJones}
The extention you have dialed is not recognized.
~ResetExtention()
-> Extention

=== SallyJones ===
You've reached John Smith.@john smith is dead^
This is a test of choices.
+ [apples]
-> chosen("apples")
+ [oranges]
-> chosen("oranges")
+ [bananas]
-> chosen("bananas")

=== chosen(choice) ===
You've chosen {choice}
~ SetAutomatedSystem(true)
Would you like to try a puzzle?<br>Press 1 for "yes"<br>Press 2 for "no"
+ [yes]
-> FirstPuzzle
+ [no]
~ SetAutomatedSystem(false)
Okay. Goodbye!
-> DONE

=== FirstPuzzle ===
~ EnterPuzzleMode(1, "0000000")
~ SetAutomatedSystem(false)
Congratulations. You solved the puzzle!
Would you like to try another one?
+ [yes]
-> SecondPuzzle
+ [no]
Okay. Goodbye!
->DONE

=== SecondPuzzle ===
~ EnterPuzzleMode(2, "0000000")
Wow. I didn't think you'd get that one.
I'll have to come with something more difficult.
Goodbye for now!

-> END 
