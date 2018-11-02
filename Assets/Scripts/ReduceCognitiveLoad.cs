using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ReduceCognitiveLoad : MonoBehaviour {
    private const string CORRECT_NEW_STATE = "CORRECT_NEW_STATE";
    private const string INCORRECT_NEW_STATE = "INCORRECT_NEW_STATE";
    private const string BROKEN_RULE_MAX2 = "BROKEN_RULE_MAX2";
    private const string BROKEN_RULE_MAX3 = "BROKEN_RULE_MAX3";
    private const string BROKEN_RULE_MAX4 = "BROKEN_RULE_MAX4";
    private const string BROKEN_RULE_CONNECTED = "BROKEN_RULE_CONNECTED";
    private int maxRule = 4;
    private int roundNumber = 1;

    public int RoundNumber
    {
        get {
            return roundNumber;
        }
        set {
            RoundNumberText.text = value.ToString();
            roundNumber = value;
        }
    }

    public int MaxRule
    {
        get
        {
            return maxRule;
        }
        set
        {
            MaxRuleText.text = value.ToString();
            deactivateAllRules();
            maxRule = value;
            checkForSolutionOrInvalidStates(WizardManager.Instance.GetBoardState());
        }
    }

    private void deactivateAllRules()
    {
        RuleMax2.SetActive(false);
        RuleMax3.SetActive(false);
        RuleMax4.SetActive(false);
        RuleConnected.SetActive(false);
    }

    public static ReduceCognitiveLoad Instance;

    //Breaking Rules 
    public GameObject RuleMax4;
    public GameObject RuleMax3;
    public GameObject RuleMax2;
    public GameObject RuleConnected;

    //Explaining Rules
    public GameObject RobotIntro;
    public GameObject GeneralIntro;
    public GameObject RephraseIntro;
    public GameObject GeneralRule1;
    public GameObject GeneralRule2;
    public GameObject RephraseRule1;
    public GameObject RephraseRule2;
    public GameObject ClearExample;

    //Round Management
    public GameObject IntroToRound1;
    public GameObject IntroToRound2;
    public GameObject IntroToRound3;
    public GameObject RephraseIntroRound1;
    public GameObject RephraseIntroRound2;
    public GameObject RephraseIntroRound3;

    //End of Round
    public GameObject AreWeDone;
    public GameObject LetsLift;
    public GameObject WellDone;
    public GameObject ClearTheBoard;
    public GameObject Sleep;

    public Text RoundNumberText;
    public Text MaxRuleText;


    public GameObject SessionCompleted;
    List<List<int>> neighbours = new List<List<int>>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        neighbours.Add(new List<int>() { 1, 4 }); //A1
        neighbours.Add(new List<int>() { 0, 2, 5 }); //A2
        neighbours.Add(new List<int>() { 1, 3, 6 }); //A3
        neighbours.Add(new List<int>() { 2, 7 }); //A4
        neighbours.Add(new List<int>() { 0, 5, 8 }); //B1
        neighbours.Add(new List<int>() { 1, 4, 6, 9 }); //B2
        neighbours.Add(new List<int>() { 2, 5, 7, 10 }); //B3
        neighbours.Add(new List<int>() { 3, 6, 11 }); //B4
        neighbours.Add(new List<int>() { 4, 9, 12 }); //C1
        neighbours.Add(new List<int>() { 5, 8, 10, 13 }); //C2
        neighbours.Add(new List<int>() { 6, 9, 11, 14 }); //C3
        neighbours.Add(new List<int>() { 7, 10, 15 }); //C4
        neighbours.Add(new List<int>() { 8, 13 }); //D1
        neighbours.Add(new List<int>() { 9, 12, 14 }); //D2
        neighbours.Add(new List<int>() { 10, 13, 15 }); //D3
        neighbours.Add(new List<int>() { 11, 14 }); //D4
        deactivateAllRules();
        NewSession();
    }

    public void BoardStateUpdated(bool[] boardState)
    {
        checkForSolutionOrInvalidStates(boardState);
    }

    private void checkForSolutionOrInvalidStates(bool[] currBoardState)
    {
        WizardManager.Instance.ClearHints();
        int numberOfPieces = 0;
        List<List<int>> listIslands = new List<List<int>>();
        bool[] visitedPositions = new bool[16];

        for (int height = 0; height < 4; height++)
        {
            for (int length = 0; length < 4; length++)
            {
                int currentPos = 4 * height + length;
                if (!visitedPositions[currentPos])
                {
                    List<int> currentIsland = new List<int>();
                    visitedPositions[currentPos] = true;
                    if (currBoardState[currentPos])
                    {
                        currentIsland.Add(currentPos);
                        checkNeighbors(currentPos, currBoardState, visitedPositions, currentIsland);
                    }
                    if (currentIsland.Count > 0)
                    {
                        listIslands.Add(currentIsland);
                        numberOfPieces += currentIsland.Count;
                    }
                }
            }
        }

        int maxElements = numMaxConnectedLine(currBoardState);


        bool notConnectedRule = listIslands.Count > 1;

        RuleConnected.SetActive(notConnectedRule);
        if (notConnectedRule)
            Dbg.Log(BROKEN_RULE_CONNECTED);

        bool exactNumberRule = true;
        bool correctSolution = true;


        //if()

        //Check if the max elements in a row or collumn are greater than the maxrule and if the number of pieces is max if it has the exact number
        if (maxElements > MaxRule || (numberOfPieces == 6 && maxElements < MaxRule))
        {

            RuleMax2.SetActive(MaxRule == 2);
            RuleMax3.SetActive(MaxRule == 3);
            RuleMax4.SetActive(MaxRule == 4);
            switch (MaxRule)
            {
                case 2:
                    Dbg.Log(BROKEN_RULE_MAX2);
                    break;
                case 3:
                    Dbg.Log(BROKEN_RULE_MAX3);
                    break;
                case 4:
                    Dbg.Log(BROKEN_RULE_MAX4);
                    break;
            }

            exactNumberRule = false;
        }
        else
        {
            RuleMax2.SetActive(false);
            RuleMax3.SetActive(false);
            RuleMax4.SetActive(false);
        }

        if (numberOfPieces == 6 && !notConnectedRule)
        {
            correctSolution = checkCorrectSolution(currBoardState);
        }
        else if (numberOfPieces > 0 && !notConnectedRule)
        {
            correctSolution = searchSolutionAndFillHints(currBoardState);
        }

        if (notConnectedRule || !exactNumberRule || !correctSolution)
        {
            Dbg.Log(INCORRECT_NEW_STATE);
            StateCorrectness.Instance.Change(false);
            return;
        }

        //If we survived here we know that the state is correct
        Dbg.Log(CORRECT_NEW_STATE);
        StateCorrectness.Instance?.Change(true);
    }

    private int numMaxConnectedLine(bool[] currBoardState)
    {
        int biggestLineCount = 0;
        for (int i = 0; i < 4; i++)
        {
            int currentLineCount = 0;
            foreach (var item in piecesInCollumn(i, currBoardState))
            {
                if (item)
                {
                    currentLineCount++;
                    if (currentLineCount > biggestLineCount)
                        biggestLineCount = currentLineCount;
                }
                else
                {
                    currentLineCount = 0;
                }

            }
            currentLineCount = 0;
            foreach (var item in piecesInRow(i, currBoardState))
            {
                if (item)
                { 
                    currentLineCount++;
                    if (currentLineCount > biggestLineCount)
                        biggestLineCount = currentLineCount;
                }
                else
                {
                    currentLineCount = 0;
                }
            }
        }
        return biggestLineCount;
    }

    private int numPiecesInCollumn(int collumn, bool[] currBoardState)
    {
        return piecesInCollumn(collumn, currBoardState).Where(c => c).Count();
    }

    private int numPiecesInRow(int row, bool[] currBoardState)
    {
        return piecesInRow(row, currBoardState).Where(r => r).Count();
    }

    private int numMatches(bool[] v1, bool[] v2)
    {
        int sum = 0;
        for (int i = 0; i < v1.Length; i++)
        {
            if (v1[i] && v2[i])
                sum++;
        }
        return sum;
    }

    private bool[] piecesInRow(int row, bool[] board)
    {
        bool[] rowArray = new bool[4];
        int index = 0;
        for (int i = row; i <= row + 12; i = i + 4)
        {
            rowArray[index] = board[i];
            index++;
        }
        return rowArray;
    }

    private bool[] piecesInCollumn(int collumn, bool[] board)
    {
        bool[] rowArray = new bool[4];
        int startingIndex = collumn * 4;
        for (int i = startingIndex; i < startingIndex + 4; i++)
        {
            rowArray[i-startingIndex] = board[i];
        }
        return rowArray;
    }

    /// <summary>
    /// Returns true if there is a solution. It also populates hints on the board.
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    private bool searchSolutionAndFillHints(bool [] board)
    {
        List<bool[]> possibleSolutions = new List<bool[]>();
        expandAndCheck(board, possibleSolutions);
        if (possibleSolutions.Count > 0)
        {
            for (int i = 0; i < 16; i++)
            {
                if (!board[i])
                {
                    bool isConnectedToIsland = false;
                    foreach (var item in neighbours[i])
                    {
                        if(board[item])
                        {
                            isConnectedToIsland = true;
                            break;
                        }
                    }
                    if (isConnectedToIsland)
                    {
                        foreach (var possibleSolution in possibleSolutions)
                        {
                            if (possibleSolution[i])
                            {
                                WizardManager.Instance.SetHint(i, true);
                                break;
                            }
                        }
                    }
                }
            }
            return true;
        }
        else return false;
    }

    
    private void expandAndCheck(bool [] currentBoard, List<bool[]> possibleSolutions)
    {
        if (numMaxConnectedLine(currentBoard) > MaxRule)
            return;

        if(currentBoard.Where(p => p).Count() == 6)
        {
            if (checkCorrectSolution(currentBoard))
                possibleSolutions.Add(currentBoard);
        }
        else
        {
            List<int> expansion = new List<int>();
            for (int i = 0; i < 16; i++)
            {
                if (currentBoard[i])
                {
                    foreach (var neighbour in neighbours[i])
                    {
                        if(!currentBoard[neighbour])
                            expansion.Add(neighbour);
                    }
                }
            }
            expansion = expansion.Distinct().ToList();
            foreach (var possibleMove in expansion)
            {
                if (!currentBoard[possibleMove])
                {
                    bool[] newBoardState = new bool[16];
                    Array.Copy(currentBoard, newBoardState, 16);
                    newBoardState[possibleMove] = true;
                    expandAndCheck(newBoardState, possibleSolutions);
                }
            }
        }
    }

    private bool checkCorrectSolution(bool [] currentBoard)
    {

        if (maxRule == 4)
        {
            for (int height = 0; height < 4; height++)
            {
                if (numPiecesInRow(height, currentBoard) == 4)
                {
                    //a line in the corner will not work
                    if (height == 0 || height == 3)
                        return false;
                    if (numPiecesInRow(height-1, currentBoard) != 1 || numPiecesInRow(height + 1, currentBoard) != 1)
                        return false;
                }
            }

            for (int length = 0; length < 4; length++)
            {
                if (numPiecesInCollumn(length, currentBoard) == 4)
                {
                    if (length == 0 || length == 3)
                        return false;
                    if (numPiecesInCollumn(length - 1, currentBoard) != 1 || numPiecesInCollumn(length + 1, currentBoard) != 1)
                        return false;
                }
            }
            if (numMaxConnectedLine(currentBoard) == MaxRule)
                return true;
            else return false;
        }
        else if (maxRule == 3)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int height = 0; height < 4; height++)
                {
                    if (numPiecesInRow(height, currentBoard) == 3)
                    {
                        //a line in the corner will not work
                        if (height == 0 || height == 3)
                            continue;

                        int bottomSum = numPiecesInRow(height - 1, currentBoard);
                        int topSum = numPiecesInRow(height + 1, currentBoard);

                        if (bottomSum == 1)
                        {

                            //if the bottom value is below one of the 3 pieces it passes our test
                            if(numMatches(piecesInRow(height-1, currentBoard), piecesInRow(height, currentBoard)) == 1)
                            {
                                if (topSum == 2)
                                {
                                    if (numMatches(piecesInRow(height + 1, currentBoard), piecesInRow(height, currentBoard)) == 1)
                                    {
                                        if (testForConnected2(piecesInRow(height+1, currentBoard)))
                                            return true;
                                    }
                                }
                            }
                        }
                        else if (topSum == 1)
                        {
                            //if the bottom value is below one of the 3 pieces it passes our test
                            if (numMatches(piecesInRow(height + 1, currentBoard), piecesInRow(height, currentBoard)) == 1)
                            {
                                if (bottomSum == 2)
                                {
                                    if (numMatches(piecesInRow(height - 1, currentBoard), piecesInRow(height, currentBoard)) == 1)
                                    {
                                        if (testForConnected2(piecesInRow(height - 1, currentBoard)))
                                            return true;
                                    }
                                }
                            }
                        }
                    }
                }

                for (int length = 0; length < 4; length++)
                {
                    if (numPiecesInCollumn(length, currentBoard) == 3)
                    {
                        //a line in the corner will not work
                        if (length == 0 || length == 3)
                            continue;

                        int leftSum = numPiecesInCollumn(length -1 , currentBoard);
                        int rightSum = numPiecesInCollumn(length + 1, currentBoard);

                        if (leftSum == 1)
                        {
                            //if the bottom value is below one of the 3 pieces it passes our test
                            if (numMatches(piecesInCollumn(length - 1, currentBoard), piecesInCollumn(length, currentBoard)) == 1)
                            {
                                if (rightSum == 2)
                                {
                                    if (numMatches(piecesInCollumn(length + 1, currentBoard), piecesInCollumn(length, currentBoard)) == 1)
                                    {
                                        if (testForConnected2(piecesInCollumn(length + 1, currentBoard)))
                                            return true;
                                    }
                                }
                            }
                        }
                        else if (rightSum == 1)
                        {
                            //if the bottom value is below one of the 3 pieces it passes our test
                            if (numMatches(piecesInCollumn(length + 1, currentBoard), piecesInCollumn(length, currentBoard)) == 1)
                            {
                                if (leftSum == 2)
                                {
                                    if (numMatches(piecesInCollumn(length - 1, currentBoard), piecesInCollumn(length, currentBoard)) == 1)
                                    {
                                        if (testForConnected2(piecesInCollumn(length - 1, currentBoard)))
                                            return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            for (int length = 0; length < 4; length++)
            {
                if (numPiecesInCollumn(length, currentBoard) == 3)
                {
                    if (length == 0 || length == 3)
                        return false;
                    if (numPiecesInCollumn(length - 1, currentBoard) != 1 || numPiecesInCollumn(length + 1, currentBoard) != 1)
                        return false;
                }
            }
            return false;
        }
        else if (MaxRule == 2)
            return true;
        else return false;
    }

    private bool testForConnected2(bool [] boolVector)
    {
        if ((boolVector[0] && boolVector[1]) || (boolVector[1] && boolVector[2]) || (boolVector[2] && boolVector[3]))
            return true;
        else return false;
    }

    private Vector4 multiplyVector(Vector4 vector41, Vector4 vector42)
    {
        return new Vector4(vector41.x * vector42.x, vector41.y * vector42.y, vector41.z * vector42.z, vector41.w * vector42.w);
    }

    private int sumVector(Vector4 vector)
    {
        return (int) (vector.x + vector.y + vector.z + vector.w);
    }

    public int sumOfVector4(Vector4 vector)
    {
        float sum = vector.x + vector.y + vector.z + vector.w;
        return (int)sum;
    }

    //Fill current island with all the neighbours that are physically there
    private void checkNeighbors(int currentPos, bool[] boardState, bool[] visitedPositions, List<int> currentIsland)
    {
        foreach (var neighbour in neighbours[currentPos])
        {
            if (!visitedPositions[neighbour])
            {
                visitedPositions[neighbour] = true;
                if (boardState[neighbour])
                {
                    currentIsland.Add(neighbour);
                    checkNeighbors(neighbour, boardState, visitedPositions, currentIsland);
                }
            }
        }
    }

    public void NewSession()
    {
        RobotIntro.SetActive(true);
        GeneralIntro.SetActive(true);
        RephraseIntro.SetActive(true);
        GeneralRule1.SetActive(true);
        GeneralRule2.SetActive(true);
        RephraseRule1.SetActive(true);
        RephraseRule2.SetActive(true);
        IntroToRound1.SetActive(true);
        IntroToRound2.SetActive(true);
        IntroToRound3.SetActive(true);
        RephraseIntroRound1.SetActive(true);
        RephraseIntroRound2.SetActive(true);
        RephraseIntroRound3.SetActive(true);
        ClearExample.SetActive(true);
        SessionCompleted.SetActive(true);
        Sleep.SetActive(true);

        MaxRule = 4;
        RoundNumber = 1;
        RuleConnected.SetActive(true);
    }

    // Update is called once per frame
    public void UpdateButtons () {
		
	}

    internal void ButtonPressed(GameObject gameObject)
    {
        if (gameObject == RobotIntro)
        {
            RobotIntro.SetActive(false);
        }
        else if (gameObject == GeneralIntro)
        {
            GeneralIntro.SetActive(false);
        }
        else if (gameObject == GeneralRule1)
        {
            GeneralRule1.SetActive(false);
            RephraseIntro.SetActive(false);
        }
        else if (gameObject == GeneralRule2)
        {
            GeneralRule2.SetActive(false);
            RephraseRule1.SetActive(false);
        }
        else if (gameObject == ClearExample)
        {
            ClearExample.SetActive(false);
            RephraseRule2.SetActive(false);
        }
        else if (gameObject == IntroToRound1)
        {
            IntroToRound1.SetActive(false);
            MaxRule = 4;
        }
        else if (gameObject == IntroToRound2)
        {
            IntroToRound2.SetActive(false);
            RephraseIntroRound1.SetActive(false);
            //IntroduceMax3.GetComponent<WizardButtonPressed>().Blink();
            RoundNumber = 2;
            RoundNumberText.text = "2";
            MaxRule = 3;
        }
        else if (gameObject == IntroToRound3)
        {
            IntroToRound3.SetActive(false);
            RephraseIntroRound2.SetActive(false);
            RoundNumber = 3;
            MaxRule = 2;
        }
        else if (gameObject == RephraseIntroRound3)
        {
            RephraseIntroRound3.SetActive(false);
        }
        else if (gameObject == SessionCompleted)
        {
            SessionCompleted.SetActive(false);
        }
        else if (gameObject == Sleep)
        {
            Sleep.SetActive(false);
            Settings.Instance.EndSession();
        }
    }
}
