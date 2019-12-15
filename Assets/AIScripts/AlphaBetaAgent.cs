using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// AI decision algorithms for Othello
/// </summary>
public class AlphaBetaAgent : AIScript {
    int maxDepth = 5;
    int maxMinimaxDepth = 3;
    public const float inf = Mathf.Infinity;
    public const float negInf = Mathf.NegativeInfinity;

    //float[,] boardWeights = { // Values to extreme maybe?
    //    {inf,    negInf, 100, 100, 100, 100, negInf, inf},    // Corners are super valuable 
    //    {negInf, negInf, 10,   10, 10,  10,  negInf, negInf}, // Spaces right next to corners are dangerous (see below)  
    //    {100,    10,      5,   5,  5,  5,    10,      100},
    //    {100,    10,      5,   1,  1,  5,    10,      100},    // Edges are pretty decent
    //    {100,    10,      5,   1,  1,  5,    10,      100},    // Rest get less valuable towards the center
    //    {100,    10,      5,   5,  5,  5,    10,      100},
    //    {negInf, negInf, 10,   10, 10, 10,   negInf, negInf},
    //    {inf,    negInf, 100, 100, 100, 100, negInf, inf}
    //};

    float[,] boardWeights = {
        {1000, 0, 100, 100, 100, 100, 0,  1000}, // Corners are super valuable 
        {0,    0,  50, 50,  50,  50,  0,     0}, // Spaces right next to corners are dangerous (see below)
        {100,  50, 30, 30,  30,  30,  50,  100}, // Edges are pretty decent
        {100,  50, 30,  1,   1,  30,  50,  100},  // Rest get less valuable towards the center
        {100,  50, 30,  1,   1,  30,  50,  100},
        {100,  50, 30, 30,  30,  30,  50,  100},
        {0,    0,  50, 50,  50,  50,   0,    0},
        {1000, 0, 100, 100, 100, 100,  0, 1000}
    };

    public override KeyValuePair<int, int> makeMove(List<KeyValuePair<int, int>> availableMoves, BoardSpace[][] currentBoard, uint turnNumber) {
        //return GetAction(currentBoard, availableMoves, turnNumber); // For AlphaBeta Minimax
        return GetBestAction(currentBoard, availableMoves, turnNumber); // For AlphaBeta Negamax
    }

    public BoardSpace[][] updateBoard(BoardSpace[][] previousBoard, KeyValuePair<int, int> move, uint turnNumber) {
        BoardSpace[][] newBoard = previousBoard.Select(x => x.ToArray()).ToArray();
        if (turnNumber % 2 == 0) newBoard[move.Key][move.Value] = BoardSpace.BLACK;
        else newBoard[move.Key][move.Value] = BoardSpace.WHITE;
        List<KeyValuePair<int, int>> updatedMoves = BoardScript.GetPointsChangedFromMove(newBoard, turnNumber, move.Value, move.Key);
        foreach (KeyValuePair<int, int> action in updatedMoves) {
            if (turnNumber % 2 == 0) newBoard[action.Key][action.Value] = BoardSpace.BLACK;
            else newBoard[action.Key][action.Value] = BoardSpace.WHITE;
        }
        ++turnNumber;
        return newBoard;
    }

    /* Compute the greatest number of pieces of your color on the board as a measure of "score" */
    public float staticEvaluationFunction(BoardSpace[][] currentBoard) {
        int blackCount = 0;
        int whiteCount = 0;
        foreach (BoardSpace[] row in currentBoard) {
            foreach (BoardSpace space in row) {
                switch (space) {
                    case (BoardSpace.BLACK):
                        blackCount++;
                        break;
                    case (BoardSpace.WHITE):
                        whiteCount++;
                        break;
                }
            }
        }
        return Math.Abs(blackCount - whiteCount);
    }

    /* Compute the greatest number of pieces of your color, accounting for the value for each spot on the board */
    public float betterEvaluationFunction(BoardSpace[][] currentBoard) {
        float blackCount = 0;
        float whiteCount = 0;
        int i = 0;
        foreach (BoardSpace[] row in currentBoard) {
            int j = 0;
            foreach (BoardSpace space in row) {
                switch (space) {
                    case (BoardSpace.BLACK):
                        blackCount += boardWeights[i, j];
                        break;
                    case (BoardSpace.WHITE):
                        whiteCount += boardWeights[i, j];
                        break;
                }
                j++;
            }
            i++;
        }
        return Math.Abs(blackCount - whiteCount);
    }

    /* Computes the greatest number of pieces of your color, 
       accounting for the value for each spot on the board, 
       as well as pieces you take along the way. */
    public float bestEvaluationFunction(BoardSpace[][] currentBoard) {
        float blackCount = 0;
        float whiteCount = 0;
        int i = 0;
        foreach (BoardSpace[] row in currentBoard) {
            int j = 0;
            foreach (BoardSpace space in row) {
                switch (space) {
                    case (BoardSpace.BLACK):
                        blackCount += boardWeights[i, j];
                        if (i != 7 && j != 7) {
                            // Bonus because the end of our diagonal line is also our color
                            if (currentBoard[i + (7 - i)][j + (7 - j)] == BoardSpace.BLACK) {
                                blackCount += boardWeights[i + (7 - i), j + (7 - j)];

                                for (int r = i; r < 8; r++) {
                                    for (int c = j; c < 8; c++) {
                                        if (currentBoard[r][c] == BoardSpace.WHITE) { // Opposite pieces in our way 
                                            blackCount += boardWeights[r, c]; // So we take their value
                                        }
                                    }
                                }
                            }

                            if (currentBoard[i][j + (7 - j)] == BoardSpace.BLACK) { // End of our current row is same color
                                whiteCount += boardWeights[i, j + (7 - j)];

                                for (int c = j; c < 8; c++) {
                                    if (currentBoard[i][c] == BoardSpace.WHITE) { // Opposite pieces in our way 
                                        whiteCount += boardWeights[i, c]; // So we take their value
                                    }
                                }
                            }

                            if (currentBoard[i + (7 - i)][j] == BoardSpace.BLACK) { // End of our current column is same color
                                whiteCount += boardWeights[i + (7 - i), j];

                                for (int r = i; r < 8; r++) {
                                    if (currentBoard[r][j] == BoardSpace.WHITE) { // Opposite pieces in our way 
                                        whiteCount += boardWeights[r, j]; // So we take their value
                                    }
                                }
                            }

                        }
                        break;

                    case (BoardSpace.WHITE):
                        whiteCount += boardWeights[i, j];
                        if (i != 8 && j != 8) {
                            // Bonus because the end of our diagonal line is also our color
                            if (currentBoard[i + (7 - i)][j + (7 - j)] == BoardSpace.WHITE) {
                                whiteCount += boardWeights[i + (7 - i), j + (7 - j)];

                                for (int r = i; r < 8; r++) {
                                    for (int c = j; c < 8; c++) {
                                        if (currentBoard[r][c] == BoardSpace.BLACK) { // Opposite pieces in our way 
                                            whiteCount += boardWeights[r, c]; // So we take their value
                                        }
                                    }
                                }
                            }

                            if (currentBoard[i][j + (7 - j)] == BoardSpace.WHITE) { // End of our current row is same color
                                whiteCount += boardWeights[i, j + (7 - j)];

                                for (int c = j; c < 8; c++) {
                                    if (currentBoard[i][c] == BoardSpace.BLACK) { // Opposite pieces in our way 
                                        whiteCount += boardWeights[i, c]; // So we take their value
                                    }
                                }
                            }

                            if (currentBoard[i + (7 - i)][j] == BoardSpace.WHITE) { // End of our current column is same color
                                whiteCount += boardWeights[i + (7 - i), j];

                                for (int r = i; r < 8; r++) {
                                    if (currentBoard[r][j] == BoardSpace.BLACK) { // Opposite pieces in our way 
                                        whiteCount += boardWeights[r, j]; // So we take their value
                                    }
                                }
                            }

                        }
                        break;
                }
                j++;
            }
            i++;
        }
        return Math.Abs(blackCount - whiteCount);
    }

    /************************************************* AlphaBeta Minimax implementation /*************************************************/

    public float Value(BoardSpace[][] curBoard, int curDepth, float alpha, float beta, uint turnNumber) {
        turnNumber++;
        List<KeyValuePair<int, int>> actions = BoardScript.GetValidMoves(curBoard, turnNumber);
        if (curDepth <= 0 || actions.Count == 0) {
            return betterEvaluationFunction(curBoard);
        }
        if (turnNumber % 2 == 0) return MaxValue(curBoard, curDepth, alpha, beta, turnNumber);
        else return MinValue(curBoard, curDepth, alpha, beta, turnNumber);
    }

    public float MinValue(BoardSpace[][] curBoard, int curDepth, float alpha, float beta, uint turnNumber) {
        float value = Mathf.Infinity;
        List<KeyValuePair<int, int>> actions = BoardScript.GetValidMoves(curBoard, turnNumber);
        foreach (KeyValuePair<int, int> action in actions) {
            BoardSpace[][] successor = updateBoard(curBoard, action, turnNumber);
            float newActionScore = Value(successor, (curDepth-1), alpha, beta, turnNumber++);
            value = Mathf.Min(value, newActionScore);
            beta = Mathf.Min(beta, value);
            if (alpha >= beta) break;
        }
        return value;
    }

    public float MaxValue(BoardSpace[][] curBoard, int curDepth, float alpha, float beta, uint turnNumber) {
        float value = Mathf.NegativeInfinity;
        List<KeyValuePair<int, int>> actions = BoardScript.GetValidMoves(curBoard, turnNumber);
        foreach (KeyValuePair<int, int> action in actions) {
            BoardSpace[][] successor = updateBoard(curBoard, action, turnNumber);
            float newActionScore = Value(successor, (curDepth-1), alpha, beta, turnNumber++);
            value = Mathf.Max(value, newActionScore);
            alpha = Mathf.Max(alpha, value);
            if (alpha >= beta) break;
        }
        return value;
    }

    public KeyValuePair<int, int> GetAction(BoardSpace[][] currentBoard, List<KeyValuePair<int, int>> availableMoves, uint turnNumber) {
        KeyValuePair<int, int> bestMove = new KeyValuePair<int, int>();
        float alpha = Mathf.NegativeInfinity;
        float beta = Mathf.Infinity;
        float bestScore = Mathf.NegativeInfinity;
        foreach (KeyValuePair<int, int> mv in availableMoves) {
            float currentBoardScore;
            BoardSpace[][] successor = updateBoard(currentBoard, mv, turnNumber);
            currentBoardScore = Value(successor, this.maxMinimaxDepth, alpha, beta, turnNumber);
            if (currentBoardScore > bestScore) {
                bestScore = currentBoardScore;
                bestMove = mv;
            }
        }
        return bestMove;
    }

    /*******************************************************************************************************************************************/

    /************************************************* AlphaBeta Negamax implementation /*************************************************/
    public KeyValuePair<int, int> GetBestAction(BoardSpace[][] currentBoard, List<KeyValuePair<int, int>> availableMoves, uint turnNumber) {
        float beta = Mathf.NegativeInfinity;
        float alpha = Mathf.Infinity;

        KeyValuePair<int, int> bestMove = new KeyValuePair<int, int>();
        KeyValuePair<float, KeyValuePair<int, int>> pair;

        float bestScore = Mathf.NegativeInfinity;
        foreach (KeyValuePair<int, int> mv in availableMoves) {
            float currentBoardScore;
            BoardSpace[][] childBoard = updateBoard(currentBoard, mv, turnNumber);
            pair = AlphaBetaNegamax(currentBoard, availableMoves, this.maxDepth, alpha, beta, turnNumber);
            currentBoardScore = pair.Key;

            if (currentBoardScore > bestScore) {
                bestScore = currentBoardScore;
                bestMove = mv;
            }
        }
        return bestMove;
    }

    private KeyValuePair<float, KeyValuePair<int, int>> AlphaBetaNegamax(BoardSpace[][] curBoard,  List<KeyValuePair<int, int>> availableMoves, int curDepth, float alpha, float beta, uint turnNumber) {
        turnNumber++;
        KeyValuePair<int, int> bestMove;
        float bestScore =  int.MinValue;;
        KeyValuePair<float, KeyValuePair<int, int>> bestPair;
        List<KeyValuePair<int, int>> actions = BoardScript.GetValidMoves(curBoard, turnNumber);

        if (curDepth <= 0 || actions.Count == 0) {
            float score = betterEvaluationFunction(curBoard);
            //float score = bestEvaluationFunction(curBoard);

            if (score < bestScore) {
                return new KeyValuePair<float, KeyValuePair<int, int>>(bestScore, bestMove);
            }
            else {
                KeyValuePair<int, int> lastAction = new KeyValuePair<int, int>(8,8);
                return new KeyValuePair<float, KeyValuePair<int, int>>(score, lastAction);
            }
            
        }        
        foreach (KeyValuePair<int, int> action in actions) {
            BoardSpace[][] successor = updateBoard(curBoard, action, turnNumber);
            KeyValuePair<float, KeyValuePair<int, int>> currentMove = AlphaBetaNegamax(successor, availableMoves, curDepth-1, -beta, -alpha, turnNumber);
            float value = currentMove.Key;
            float currentScore = -value;

            if (currentScore >= bestScore) {
                bestScore = currentScore;
                bestMove = action;
            }
            if (alpha < currentScore) alpha = currentScore;  
            if (alpha >= beta) break; 
        }
        bestPair = new KeyValuePair<float, KeyValuePair<int, int>>(bestScore, bestMove);
        return bestPair;
    }
    /************************************************************************************************/

}

