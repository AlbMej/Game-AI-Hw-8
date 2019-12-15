# Game AI Homework 8: Othello

## Team

* Alberto Meija
* Jessica Licther

## Notes

This implementation of Othello contans two AI's using different search tree algorithms. One of them is Minimax with Alpha Beta pruning and the other is Negamax with Alpha Beta pruning. You can change between algorithms in the AlphaBetaAgent.cs file in the makeMove function. Just comment/uncomment between the two lines there. 

There are also three evaluation functions, static, better, and best. My staticEvaluationFunction simply counts the amounts of blacks pieces and white pieces and returns the aboslute difference. This one follows the first tip you gave (Compute the greatest number of pieces of your color on the board as a measure of "score"). 
The betterEvaluationFunction does the samething but uses a board with weights to add to the individual scores, also based on the tips you gave us (corner squares are ultra-desirable, Each of the squares are given its own desirability score). 
The last SEF does not really seem to work as I had hoped. For that reason, I left the calls in the code to be betterEvaluationFunction. Basically, bestEvaluationFunction tries to do the part where you say "do some research and see what else you can come up with". This SEF include everything from the betterEvaluationFunction, but also includes the fact that if we choose a spot and there is a same color piece at the end of that line (verticaly, horizontally, or diagonally) then we also add to our score. In addition to that, if there are many white pieces in between that line, we also add to our score since that gives us more pieces on the board while removing the opposing players pieces. . 