INCLUDE ../globals.ink

{testcall_Count == 0: -> main}

=== main ===
Hello.
This is a test.
This is a test of choices.
 + [choice 1]
  -> chosen("the first choice")
 + [choice 2]
  -> chosen("the second choice")
  
 === chosen(choice) ===
 
You've chosen {choice}

-> END 
