using System;
using ChessEngine;


public class Test {
    void AskExit() {
        Console.Write("\n\nAre you sure? (y/n) > ");
        string answer = Console.ReadLine();

        if (answer == "Y" || answer == "y") {
            System.Environment.Exit(0);
        }
    }

    public static void Main(String[] args) {

        ChessEngine chess = new ChessEngine();
        FrontEnd frontEnd = new FrontEnd();
        string[] sideNames = {"White", "Black"};
        string message = "";

        while (true) {
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n");

            frontEnd.Render(chess.pieces);

            if (chess.checkmate) {
                Console.WriteLine("\n\n\u001b[31mCheckmate!\u001b[0m");
                System.Environment.Exit(0);
            }

            Console.WriteLine(message);

            message = "";

            Console.WriteLine($"\n\n{sideNames[chess.turnCounter.turn]}'s turn");

            try {
                Console.Write("\n\nSelect a piece > ");
                string Selection = Console.ReadLine();
                if (Selection == "exit") {
                    AskExit();
                }

                string[] selection = Selection.Split(" ");

                for (int x = 0; x < selection.Length; x++) {
                    selection[x] = int.Parse(selection[x]);
                }

                Console.Write("\nWhere to move to > ");
                string NewPosition = Console.ReadLine();
                if (NewPosition == "exit") {
                    AskExit();
                }

                string[] newPosition = NewPosition.Split(" ");
                for (int x = 0; x < newPosition.Length; x++) {
                    newPosition[x] = int.Parse(newPosition[x]);
                }

                Vector selectionVector = new Vector(selection[0], selection[1]);
                Vector newPositionVector = new Vector(newPosition[0], newPosition[1]);

                try {
                    bool result = chess.MovePiece(selectionVector, newPositionVector);

                    if (!result) {
                        message = "\n\n\u001b[31mYou can't do that here\u001b[0m";

                    } else {
                        message = "";
                    }
                } catch (NoSuchPieceException) {
                    message = "\n\n\u001b[31mThere is no piece there!\u001b[0m";
                }

            } catch (FormatException) {
                message = "\n\n\u001b[31mInvalid entry!\u001b[0m";
            }
        }
    }
}