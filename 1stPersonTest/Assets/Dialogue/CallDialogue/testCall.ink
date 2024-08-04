INCLUDE ../globals.ink

{testcall_Count == 0: -> main}

=== main ===
Hello.
This is a test.
This is a test of choices.
+ [apples]
-> chosen("apples")
+ [oranges]
-> chosen("oranges")
+ [bananas]
-> chosen("bananas")

=== chosen(choice) ===
You've chosen {choice}
Would you like to try a puzzle?
+ [yes]
-> FirstPuzzle
+ [no]
Okay. Goodbye!
-> DONE

=== FirstPuzzle ===
~ enterPuzzleMode(1, "0000000")
Congratulations. You solved the puzzle!
Would you like to try another one?
+ [yes]
-> SecondPuzzle
+ [no]
Okay. Goodbye!
->DONE

=== SecondPuzzle ===
~ enterPuzzleMode(2, "0000000")
Wow. I didn't think you'd get that one.
I'll have to come with something more difficult.
Goodbye for now!

-> END 
