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
    public const float inf = Mathf.Infinity;
    public const float negInf = Mathf.NegativeInfinity;

    //float[,] boardWeights = {
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
        {1000, 0, 100, 100, 100, 100, 0,  1000},
        {0,    0,  50, 50,  50,  50,  0,     0},
        {100,  50, 30, 30,  30,  30,  50,  100},
        {100,  50, 30,  1,   1,  30,  50,  100},
        {100,  50, 30,  1,   1,  30,  50,  100},
        {100,  50, 30, 30,  30,  30,  50,  100},
        {0,    0,  50, 50,  50,  50,   0,    0},
        {1000, 0, 100, 100, 100, 100,  0, 1000}
    };

    public override KeyValuePair<int, int> makeMove(List<KeyValuePair<int, int>> availableMoves, BoardSpace[][] currentBoard, uint turnNumber) {
        //return GetAction(currentBoard, availableMoves, turnNumber);
        //return findBestMove(currentBoard, availableMoves, turnNumber);
        return GetBestAction(currentBoard, availableMoves, turnNumber);
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

    /************************************************* failed implementation /*************************************************/

    public float Value(BoardSpace[][] curBoard, int curDepth, float alpha, float beta, uint turnNumber) {
        List<KeyValuePair<int, int>> actions = BoardScript.GetValidMoves(curBoard, turnNumber);
        if (curDepth <= 0 || curDepth == maxDepth || actions.Count == 0) {
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
            float newActionScore = Value(curBoard, (curDepth - 1), alpha, beta, turnNumber);
            value = Mathf.Min(value, newActionScore);
            float currentScore = Value(curBoard, (curDepth - 1), alpha, beta, ++turnNumber);
            //if (currentScore > value) maxScore = currentScore;
            if (value < alpha) return value;
            beta = Mathf.Min(beta, value);

            //if (newActionScore < value) value = newActionScore;
            //if (newActionScore < beta) newActionScore = beta;
            //if (alpha >= beta) break;
        }
        return value;
    }

    public float MaxValue(BoardSpace[][] curBoard, int curDepth, float alpha, float beta, uint turnNumber) {
        float value = Mathf.NegativeInfinity;
        List<KeyValuePair<int, int>> actions = BoardScript.GetValidMoves(curBoard, turnNumber);
        foreach (KeyValuePair<int, int> action in actions) {
            BoardSpace[][] successor = updateBoard(curBoard, action, turnNumber);
            float newActionScore = Value(curBoard, (curDepth - 1), alpha, beta, turnNumber);
            value = Mathf.Min(value, newActionScore);
            //int currentScore = value(curBoard, (curDepth - 1), alpha, beta, turnNumber);
            if (value > beta) return value;
            alpha = Mathf.Max(alpha, value);

            //if (newActionScore > value) newActionScore = value;
            //if (newActionScore > alpha) alpha = newActionScore;
            //if (alpha >= beta) break;
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
            BoardSpace[][] mvBoard = updateBoard(currentBoard, mv, turnNumber);
            currentBoardScore = Value(currentBoard, (this.maxDepth - 1), alpha, beta, ++turnNumber);
            if (currentBoardScore > bestScore) {
                bestScore = currentBoardScore;
                bestMove = mv;
            }
        }
        return bestMove;
    }

    /************************************************* failed implementation /*************************************************/

    /************************************************* better implementation /*************************************************/
    public KeyValuePair<int, int> GetBestAction(BoardSpace[][] currentBoard, List<KeyValuePair<int, int>> availableMoves, uint turnNumber) {
        float beta = Mathf.NegativeInfinity;
        float alpha = Mathf.Infinity;
        //return AlphaBetaNegamax(currentBoard, availableMoves, this.maxDepth, alpha, beta, turnNumber).Value;

        KeyValuePair<int, int> bestMove = new KeyValuePair<int, int>();
        KeyValuePair<float, KeyValuePair<int, int>> pair;

        float bestScore = Mathf.NegativeInfinity;
        foreach (KeyValuePair<int, int> mv in availableMoves) {
            float currentBoardScore;
            BoardSpace[][] mvBoard = updateBoard(currentBoard, mv, turnNumber);
            //currentBoardScore = NegamaxAB(currentBoard, 0, alpha, beta, turnNumber, availableMoves).Key;
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
        float bestScore = Mathf.NegativeInfinity;
        KeyValuePair<float, KeyValuePair<int, int>> bestPair;
        List<KeyValuePair<int, int>> actions = BoardScript.GetValidMoves(curBoard, turnNumber);

        if (curDepth <= 0 || actions.Count == 0) {
            float score = betterEvaluationFunction(curBoard);
            if (score < bestScore) {
                KeyValuePair<int, int> lastAction = new KeyValuePair<int, int>(-1, -1);
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
        //return new KeyValuePair<float, KeyValuePair<int, int>>(bestScore, bestMove);
        return bestPair;
    }
    /************************************************* better implementation /*************************************************/

}

