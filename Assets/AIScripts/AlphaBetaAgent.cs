using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// AI decision algorithms for Othello
/// </summary>
public class AlphaBetaAgent : AIScript {
    int maxDepth = 2;

    public override KeyValuePair<int, int> makeMove(List<KeyValuePair<int, int>> availableMoves, BoardSpace[][] currentBoard, uint turnNumber) {
        return GetAction(currentBoard, availableMoves, turnNumber);
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
    
    /* Compute the greatest number of pieces of your color on the board as a measure of “score” */
    public int staticEvaluationFunction(BoardSpace[][] currentBoard) {
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

    public float Value(BoardSpace[][] curBoard, int curDepth, float alpha, float beta, uint turnNumber) {
        List<KeyValuePair<int, int>> actions = BoardScript.GetValidMoves(curBoard, turnNumber);
        if (curDepth <= 0 || curDepth == maxDepth || actions.Count == 0) {
            return staticEvaluationFunction(curBoard);
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
            //float currentScore = Value(curBoard, (curDepth - 1), alpha, beta, turnNumber);
            //if (currentScore > value) maxScore = currentScore;
            if (value < alpha) return value;
            beta = Mathf.Min(beta, value);
            //if (beta <= alpha) break;
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
        }
        return value;
    }

    public KeyValuePair<int, int> GetAction(BoardSpace[][] currentBoard, List<KeyValuePair<int, int>> availableMoves, uint turnNumber) {
        KeyValuePair<int, int> bestMove = new KeyValuePair<int, int>();

        float alpha = Mathf.NegativeInfinity;
        float beta = Mathf.Infinity;
        //float alphaBetaAction = this.MaxValue(currentBoard, maxDepth, alpha, beta, turnNumber);
        //return alphaBetaAction;

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
}

