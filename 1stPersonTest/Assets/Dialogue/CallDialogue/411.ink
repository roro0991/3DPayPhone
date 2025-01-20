INCLUDE ../Globals.ink

-> MAINMENU

== MAINMENU ==
~ EnterDirectoryMode()
@
~ SetAutomatedSystem(true)
+ [Dial 1]
-> RESIDENTIAL
->DONE
+ [Dial 2]
-> BUSINESS
+ [Dial 3]
~ ExitDirectoryMode()
-> MAINMENU
->DONE

== RESIDENTIAL ==
~ PlayAudioClip(1, 2)
~ ResidentialListing()
+ [Dial 1]
->DONE
+ [Dial 2]
->DONE
+ [Dial 3]
~ ExitDirectoryMode()
-> MAINMENU
->DONE
== BUSINESS ==
~ PlayAudioClip(1, 3)
~ BusinessListing()
+ [Dial 1]
->DONE
+ [Dial 2]
->DONE
+ [Dial 3]
~ ExitDirectoryMode()
-> MAINMENU
->DONE

->END
