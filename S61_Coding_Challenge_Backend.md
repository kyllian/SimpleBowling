# Interview Coding Challenge

Create a bowling scoring program according to the rules of play, frame scoring, and technical requirements in this document. You will decide how complex the user interface and logic need to be in order to meet the requirements efficiently and effectively, and you can store the data in any format you want (in-memory, local file, database, etc.). In a follow up meeting with the team, you will explain why you settled on your solution, describe the design, and walk us through the code.

You are welcome to create any type of design documentation needed for your development process. This is optional, but it may help you in our discussion.

## Rules of play

A game of bowling consists of ten frames. In each frame, the bowler will have two chances to knock down as many pins as possible with their bowling ball. In games with more than one bowler, every bowler will take his or her frame in a predetermined order before the next frame begins. If a bowler is able to knock down all ten pins with the first ball, it is known as a strike (typically rendered as an "X" on the score sheet). If the bowler is able to knock down all 10 pins with the two balls of a frame, it is known as a spare (typically rendered as a "/" on the score sheet instead of the second pin count). Bonus points are awarded for both of these, depending on what is scored in the next 2 balls (for a strike) or 1 ball (for a spare). If the bowler knocks down all 10 pins in the tenth frame, the bowler is allowed to throw 3 balls for that frame. This allows for a potential of 12 strikes in a single game, and a maximum score of 300 points, a perfect game.

## Scoring a frame

* When a player fails to knock down all ten pins after their second ball, it is known as an open frame. One point is scored for each pin that is knocked over.
* When a player knocks down all ten pins ten pins with their first ball, it is known as a strike. Ten points are scored, plus a bonus of whatever is scored with the next two balls.
* When a player knocks down all ten pins after their second ball, it is known as a spare. Ten points are scored, plus a bonus of whatever is scored with the next ball.

## Technical requirements

* The solution UI may be as simple (e.g. console app) or complex (e.g. web app) as you prefer
* The core logic should be written in C#, but if you prefer a more complex UI, you may use any other additional languages necessary  
* The solution should require at least two bowlers per game
* The solution should allow the user to enter scores as the game progresses
* The solution should show the running score for each bowler as the game progresses
* The solution should show the final score for each bowler when the game finishes

Please let us know if you have any questions. We look forward to seeing your solution!
