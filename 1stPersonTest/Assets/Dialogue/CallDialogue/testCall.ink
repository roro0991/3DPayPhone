INCLUDE ../globals.ink

{testcall_Count == 0: -> main}
{testcall_Count == 1: -> second_call}
{testcall_Count == 3: -> third_call}

=== main ===
hello there
~ inDisplayMessageMode = true
dontreact.
heislistening.
buthecantseethis.
donttrusthim.
~ inDisplayMessageMode = false
make a choice
    + [choice 1]
        -> chosen("choice 1")
    + [choice 2]
        -> chosen("choice 2")
    + [choice 3]
        -> chosen("choice 3")
    
=== chosen(choice) ===
You chose {choice}! <br>How about another choice?
    + [choice 4]
        -> chosen2("choice 4")
    + [choice 5]
        -> chosen2("choice 5")
        
=== chosen2(choice) ===
~ testcall_Count = 1
You chose {choice}!
-> END

=== second_call ===

You again? <br> Do you want more choices?
Okay, here's another one.
 + [choice 6]
    -> chosen3("choice 6")
 + [choice 7]
    -> chosen3("choice 7")
    
=== chosen3(choice) ===
~ testcall_Count = 3
What a surprise. <br> You chose another choice.
Bahbye for now.
-> END

=== third_call ===

Aren't you tired of making choices?
 + [No]
    -> endthirdcall
 + [Yes]
    -> enterpuzzle

=== endthirdcall ===

Well I am. Goodbye.
->END

=== enterpuzzle ===

Okay then how about a puzzle?
+ [Okay]
    -> activatepuzzle
    
=== activatepuzzle ===
~ firstpuzzle_Active = true

-> END 
