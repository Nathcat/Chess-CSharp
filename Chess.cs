/*
 * ChessEngine.cs
 *
 * A Chess engine written in C#.
 *
 * @author Nathan "Nathcat" Baines
 */

using System;
using System.Collections;


namespace Chess {
    public class NoSuchPieceException : System.Exception {
        /*
         * Thrown whenever a piece cannot be found in the given position.
         */

        public override string ToString() {
            /*
             * Called when the class is cast to a string
             * :return: Error message
             */

            return "The selected piece does not exist.";
        }
    }

    public class Vector {
        /*
         * Vector class to represent locations on the board and movements of pieces.
         */

        public float x;
        public float y;

        public Vector(float X=0, float Y=0) {
             x = X;
             y = Y;
        }

        public Vector(int X=0, int Y=0) {
             x = (float) X;
             y = (float) Y;
        }

        public static Vector operator +(Vector a, Vector b) {
            return new Vector(a.x + b.x, a.y + b.y);
        }

        public static Vector operator -(Vector a, Vector b) {
            return new Vector(a.x - b.x, a.y - b.y);
        }

        public static Vector operator *(Vector a, float b) {
            return new Vector(a.x * b, a.y * b);
        }

        public static Vector operator /(Vector a, float b) {
            return new Vector(a.x / b, a.y / b);
        }

        public static bool operator ==(Vector a, Vector b) {
            return (a.x == b.x) && (a.y == b.y);
        }

        public static bool operator !=(Vector a, Vector b) {
          return !((a.x == b.x) && (a.y == b.y));
        }

        public string ToString() {
          return $"({x}, {y})";
        }

        public bool IsOutOfBounds() {
            return (x >= 8f || x <= -1f) || (y >= 8f || y <= -1f);
        }
    }

    public class Piece {
        public string name;
        public int side;
        public Vector position;
        public Vector[][] moves;
        public Vector[][] attacks;
        public int index;

        public virtual bool Move(Vector newPosition, ChessEngine engine) {
            /*
             * Try to move this piece to a new position.
             * :param newPosition: The new position of the piece.
             * :param engine: The ChessEngine managing the game.
             * :return: True/False, depending on whether or not the move was successful.
             */

            if (!engine.inCheck) {
                Vector[] legalMoves = GetLegalMoves(engine);
                bool valid = false;
                foreach (Vector move in legalMoves) {
                    if (move == newPosition) {
                        valid = true;
                        break;
                    }
                }

                if (valid) {
                    position = newPosition;
                }

                return valid;

            } else {
                Vector[] moves = GetLegalCheckMoves(engine);

                bool valid = false;
                foreach (Vector move in moves) {
                    if (move == newPosition) {
                        valid = true;
                        break;
                    }
                }

                if (valid && !newPosition.IsOutOfBounds()) {
                    position = newPosition;
                    return true;

                } else {
                    return false;
                }
            }
        }

        public virtual bool Attack(Vector newPosition, Piece attackedPiece, ChessEngine engine) {
            /*
             * Attempt to attack a piece.
             * :param newPosition: The position this piece should move to.
             * :param attackedPiece: The piece this move would attack
             * :param engine: The ChessEngine managing the game
             * :return: True/False, depending on whether or not the move was successful
             */

            if (!engine.inCheck) {
                Vector[] legalAttacks = GetLegalAttacks(false, engine);

                bool valid = false;
                foreach (Vector attack in legalAttacks) {
                    if (attack == newPosition) {
                        valid = true;
                        break;
                    }
                }

                if (valid && attackedPiece.side != side) {
                    position = newPosition;
                }

                return valid && attackedPiece.side != side;

            } else {
                Vector[] attacks = GetLegalCheckAttacks(engine);

                bool valid = false;
                foreach (Vector attack in attacks) {
                    if (attack == newPosition) {
                        valid = true;
                        break;
                    }
                }

                if (valid) {
                    position = newPosition;
                }

                return valid;
            }
        }

        public virtual Vector[] GetLegalMoves(ChessEngine engine) {
            /*
             * Get all the legal moves for this piece in the current game state.
             * :param pieces: A list of all pieces on the board.
             * :param engine: The ChessEngine managing the game.
             * :return: A list of possible moves this piece could make.
             */

            ArrayList legalMoves = new ArrayList();

            foreach (Vector[] moveSet in moves) {
                foreach (Vector move in moveSet) {
                    bool valid = true;

                    foreach (Piece piece in engine.pieces) {
                        if (piece == null) {
                            continue;
                        }

                        if (piece.index == index) {
                            continue;
                        }

                        if (piece.position == position + move) {
                            valid = false;
                        }
                    }

                    if ((position + move).IsOutOfBounds()) {
                        valid = false;
                    }

                    if (engine.GetPieceByPosition(position + move) != null) {
                      valid = false;
                    }

                    if (valid) {
                      Vector oldPosition = new Vector(position.x, position.y);
                      position += move;
                      engine.CheckForCheck();
                      valid = !engine.inCheck;
                      position = oldPosition;
                      engine.CheckForCheck();
                    }

                    if (valid) {
                        legalMoves.Add(position + move);

                    } else {
                        break;
                    }
                }
            }

            Vector[] legalMoves_out = new Vector[legalMoves.ToArray().Length];
            for (int x = 0; x < legalMoves.ToArray().Length; x++) {
                legalMoves_out[x] = (Vector) (legalMoves.ToArray()[x]);
            }

            return legalMoves_out;
        }

        public virtual Vector[] GetLegalCheckMoves(ChessEngine engine) {
            /*
             * Get all the legal moves for this piece in the current game state, given that the king is in check.
             * :param pieces: A list of all pieces on the board.
             * :param engine: The ChessEngine managing the game.
             * :return: A list of possible moves this piece could make.
             */

            ArrayList moves = new ArrayList();

            foreach (Vector move in GetLegalMoves(engine)) {
                Vector oldPosition = new Vector(position.x, position.y);
                position = move;
                engine.CheckForCheck();
                position = oldPosition;

                if (!engine.inCheck && !move.IsOutOfBounds() && engine.GetPieceByPosition(move) == null) {
                    moves.Add(move);
                }

                engine.CheckForCheck();
            }

            Vector[] legalMoves = new Vector[moves.ToArray().Length];
            for (int x = 0; x < moves.ToArray().Length; x++) {
                legalMoves[x] = (Vector) (moves.ToArray()[x]);
            }

            return legalMoves;
        }

        public virtual Vector[] GetLegalAttacks(bool friendlyFire, ChessEngine engine) {
            /*
             * Get all the legal attacks this piece can make in the current game state.
             * :param friendlyFire: Whether or not attacks on this pieces teammates should be included.
             * :param engine: The ChessEngine managing the game.
             * :return: A list of possible attacks for this piece.
             */

            ArrayList legalAttacks = new ArrayList();

            foreach (Vector[] attackSet in attacks) {
                foreach (Vector attack in attackSet) {
                    Piece piece = engine.GetPieceByPosition(position + attack);

                    if (piece != null) {
                        if (piece.index == index) {
                            continue;
                        }

                        if (piece.side == side && !friendlyFire) {
                            break;
                        }

                        legalAttacks.Add(position + attack);
                        break;
                    }
                }
            }

            Vector[] legalAttacks_out = new Vector[legalAttacks.ToArray().Length];
            for (int x = 0; x < legalAttacks.ToArray().Length; x++) {
                legalAttacks_out[x] = (Vector) (legalAttacks.ToArray()[x]);
            }

            return legalAttacks_out;
        }

        public virtual Vector[] GetLegalCheckAttacks(ChessEngine engine) {
            /*
             * Get all the possible attacks this piece can make, given that the king is in check.
             * :param engine: The ChessEngine managing the game.
             * :return: A list of possible attacks for this piece.
             */

            ArrayList attacks = new ArrayList();

            foreach (Piece threat in engine.threats) {
                if (threat == null) {
                    continue;
                }

                foreach (Vector attack in GetLegalAttacks(false, engine)) {
                    Piece[] threats = engine.GetThreats(index, attack);
                    int total = 0;
                    for (int x = 0; x < threats.Length; x++) {
                        if (threats[x].side != side) {
                            total++;
                        }
                    }

                    if (threat.position == attack && !attack.IsOutOfBounds() && total == 0) {
                        attacks.Add(attack);
                    }
                }
            }

            Vector[] legalAttacks = new Vector[attacks.ToArray().Length];
            for (int x = 0; x < attacks.ToArray().Length; x++) {
                legalAttacks[x] = (Vector) (attacks.ToArray()[x]);
            }

            return legalAttacks;
        }

        public virtual bool ShouldPromote() {
            return false;
        }

        public override string ToString() {
            /*
             * Cast this piece to a string.
             * :return: A string representation of this piece with coloured text.
             */

            string colour = "";
            if (side == 0) {
                colour = "\u001b[37m";

            } else {
                colour = "\u001b[38;5;247m";
            }

            return $"{colour}{name}\u001b[0m";
        }
    }

    public class Pawn : Piece {
        public Pawn(int Index, int Side, Vector Position) {
            side = Side;
            int direction = 1;
            if (side == 1) {
                direction = -1;
            }

            index = Index;
            name = "Pa";
            position = Position;
            moves = new Vector[][] {
                new Vector[] {
                  new Vector(0, 1 * direction),
                  new Vector(0, 2 * direction)
                }
            };

            attacks = new Vector[][] {
                new Vector[] {new Vector(-1, 1 * direction)},
                new Vector[] {new Vector(1, 1 * direction)}
            };
        }

        public override bool Move(Vector newPosition, ChessEngine engine) {
            int direction = 1;
            if (side == 1) {
                direction = -1;
            }

            bool result = base.Move(newPosition, engine);

            if (result) {
                moves = new Vector[][] { new Vector[] { new Vector(0, 1 * direction)} };
            }

            return result;
        }

        public override bool ShouldPromote()
        {
            int direction = 1;
            if (side == 1) {
                direction = -1;
            }

            if (direction == 1 && position.y == 7) {
                return true;

            } else if (direction == -1 && position.y == 0) {
                return true;

            } else {
                return false;
            }
        }
    }

    public class Rook : Piece {
        public Rook(int Index, int Side, Vector Position) {
            Vector[][] Moves = new Vector[][] {
                new Vector[] {
                    new Vector(-1, 0),
                    new Vector(-2, 0),
                    new Vector(-3, 0),
                    new Vector(-4, 0),
                    new Vector(-5, 0),
                    new Vector(-6, 0),
                    new Vector(-7, 0)
                },

                new Vector[] {
                    new Vector(1, 0),
                    new Vector(2, 0),
                    new Vector(3, 0),
                    new Vector(4, 0),
                    new Vector(5, 0),
                    new Vector(6, 0),
                    new Vector(7, 0)
                },

                new Vector[] {
                    new Vector(0, 1),
                    new Vector(0, 2),
                    new Vector(0, 3),
                    new Vector(0, 4),
                    new Vector(0, 5),
                    new Vector(0, 6),
                    new Vector(0, 7)
                },

                new Vector[] {
                    new Vector(0, -1),
                    new Vector(0, -2),
                    new Vector(0, -3),
                    new Vector(0, -4),
                    new Vector(0, -5),
                    new Vector(0, -6),
                    new Vector(0, -7)
                }
            };

            index = Index;
            name = "Ro";
            side = Side;
            position = Position;
            moves = Moves;
            attacks = Moves;
        }
    }

    public class Bishop : Piece {
        public Bishop(int Index, int Side, Vector Position) {
            Vector[][] Moves = new Vector[][] {
                new Vector[] {
                    new Vector(1, 1),
                    new Vector(2, 2),
                    new Vector(3, 3),
                    new Vector(4, 4),
                    new Vector(5, 5),
                    new Vector(6, 6),
                    new Vector(7, 7)
                },

                new Vector[] {
                    new Vector(-1, 1),
                    new Vector(-2, 2),
                    new Vector(-3, 3),
                    new Vector(-4, 4),
                    new Vector(-5, 5),
                    new Vector(-6, 6),
                    new Vector(-7, 7)
                },

                new Vector[] {
                    new Vector(1, -1),
                    new Vector(2, -2),
                    new Vector(3, -3),
                    new Vector(4, -4),
                    new Vector(5, -5),
                    new Vector(6, -6),
                    new Vector(7, -7)
                },

                new Vector[] {
                    new Vector(-1, -1),
                    new Vector(-2, -2),
                    new Vector(-3, -3),
                    new Vector(-4, -4),
                    new Vector(-5, -5),
                    new Vector(-6, -6),
                    new Vector(-7, -7)
                }
            };

            index = Index;
            name = "Bi";
            side = Side;
            position = Position;
            moves = Moves;
            attacks = Moves;
        }
    }

    public class Knight : Piece {
        public Knight(int Index, int Side, Vector Position) {
            Vector[][] Moves = new Vector[][] {
                new Vector[] { new Vector(1, 2) },
                new Vector[] { new Vector(2, 1) },
                new Vector[] { new Vector(2, -1) },
                new Vector[] { new Vector(1, -2) },
                new Vector[] { new Vector(-1, -2) },
                new Vector[] { new Vector(-2, -1) },
                new Vector[] { new Vector(-2, 1) },
                new Vector[] { new Vector(-1, 2) }
            };

            index = Index;
            name = "Kn";
            side = Side;
            position = Position;
            moves = Moves;
            attacks = Moves;
        }
    }

    public class Queen : Piece {
        public Queen(int Index, int Side, Vector Position) {
            Vector[][] Moves = new Vector[][] {
                new Vector[] {
                    new Vector(-1, 0),
                    new Vector(-2, 0),
                    new Vector(-3, 0),
                    new Vector(-4, 0),
                    new Vector(-5, 0),
                    new Vector(-6, 0),
                    new Vector(-7, 0)
                },

                new Vector[] {
                    new Vector(1, 0),
                    new Vector(2, 0),
                    new Vector(3, 0),
                    new Vector(4, 0),
                    new Vector(5, 0),
                    new Vector(6, 0),
                    new Vector(7, 0)
                },

                new Vector[] {
                    new Vector(0, 1),
                    new Vector(0, 2),
                    new Vector(0, 3),
                    new Vector(0, 4),
                    new Vector(0, 5),
                    new Vector(0, 6),
                    new Vector(0, 7)
                },

                new Vector[] {
                    new Vector(0, -1),
                    new Vector(0, -2),
                    new Vector(0, -3),
                    new Vector(0, -4),
                    new Vector(0, -5),
                    new Vector(0, -6),
                    new Vector(0, -7)
                },

                                new Vector[] {
                    new Vector(1, 1),
                    new Vector(2, 2),
                    new Vector(3, 3),
                    new Vector(4, 4),
                    new Vector(5, 5),
                    new Vector(6, 6),
                    new Vector(7, 7)
                },

                new Vector[] {
                    new Vector(-1, 1),
                    new Vector(-2, 2),
                    new Vector(-3, 3),
                    new Vector(-4, 4),
                    new Vector(-5, 5),
                    new Vector(-6, 6),
                    new Vector(-7, 7)
                },

                new Vector[] {
                    new Vector(1, -1),
                    new Vector(2, -2),
                    new Vector(3, -3),
                    new Vector(4, -4),
                    new Vector(5, -5),
                    new Vector(6, -6),
                    new Vector(7, -7)
                },

                new Vector[] {
                    new Vector(-1, -1),
                    new Vector(-2, -2),
                    new Vector(-3, -3),
                    new Vector(-4, -4),
                    new Vector(-5, -5),
                    new Vector(-6, -6),
                    new Vector(-7, -7)
                }
            };

            index = Index;
            name = "Qu";
            side = Side;
            position = Position;
            moves = Moves;
            attacks = Moves;
        }
    }

    public class King : Piece {
        public King(int Index, int Side, Vector Position) {
            Vector[][] Moves = new Vector[][] {
                new Vector[] { new Vector(-1, 1) },
                new Vector[] { new Vector(0, 1) },
                new Vector[] { new Vector(1, 1) },
                new Vector[] { new Vector(1, 0) },
                new Vector[] { new Vector(1, -1) },
                new Vector[] { new Vector(0, -1) },
                new Vector[] { new Vector(-1, -1) },
                new Vector[] { new Vector(-1, 0) }
            };

            index = Index;
            name = "Ki";
            side = Side;
            position = Position;
            moves = Moves;
            attacks = Moves;
        }

        public override Vector[] GetLegalCheckAttacks(ChessEngine engine) {
            ArrayList attacks = new ArrayList();

            foreach (Piece threat in engine.threats) {
                foreach (Vector attack in GetLegalAttacks(false, engine)) {
                    Piece[] threats = engine.GetThreats(index, attack);

                    bool emptyPass = false;
                    while (!emptyPass) {
                        emptyPass = true;
                        for (int x = 0; x < threats.Length; x++) {
                            if (threats[x].side == side) {
                                threats[x] = null;
                                emptyPass = false;
                                break;
                            }
                        }
                    }

                    int total = 0;
                    foreach (Piece piece in threats) {
                        if (piece == null) {
                            continue;
                        }

                        total++;
                    }

                    if (total == 0) {
                        attacks.Add(attack);
                    }
                }
            }

            Vector[] legalAttacks = new Vector[attacks.ToArray().Length];
            for (int x = 0; x < attacks.ToArray().Length; x++) {
                legalAttacks[x] = (Vector) (attacks.ToArray()[x]);
            }

            return legalAttacks;
        }
    }

    public class TurnCounter {
        public int turn = 0;

        public void Toggle() {
            if (turn == 0) {
                turn = 1;

            } else {
                turn = 0;
            }
        }
    }

    public class FrontEnd {
        public void Render(Piece[] pieces) {
            string[][] board = new string[][] {
                new string[8],
                new string[8],
                new string[8],
                new string[8],
                new string[8],
                new string[8],
                new string[8],
                new string[8],
            };

            for (int y = 0; y < 8; y++) {
                for (int x = 0; x < 8; x++) {
                    bool found = false;
                    foreach (Piece piece in pieces) {
                        if (piece == null) {
                            continue;
                        }

                        if (piece.position == new Vector(x, y)) {
                            board[y][x] = piece.ToString();
                            found = true;
                            break;
                        }
                    }

                    if (!found) {
                        board[y][x] = "  ";
                    }
                }
            }

            Console.WriteLine("  0  1  2  3  4  5  6  7");
            for (int y = 0; y < 8; y++) {
                string str = $"{y}|";

                for (int x = 0; x < 8; x++) {
                    str += board[y][x] + "|";
                }

                Console.WriteLine(str);
            }
        }
    }

    public class ChessEngine {
        public bool inCheck;
        public Piece[] pieces;
        public Piece[] threats;
        public bool checkmate;
        public TurnCounter turnCounter;

        public ChessEngine() {
            inCheck = false;
            checkmate = false;
            turnCounter = new TurnCounter();
            threats = new Piece[] {};

            pieces = new Piece[] {
                new Pawn(0, 0, new Vector(0, 1)),
                new Pawn(1, 0, new Vector(1, 1)),
                new Pawn(2, 0, new Vector(2, 1)),
                new Pawn(3, 0, new Vector(3, 1)),
                new Pawn(4, 0, new Vector(4, 1)),
                new Pawn(5, 0, new Vector(5, 1)),
                new Pawn(6, 0, new Vector(6, 1)),
                new Pawn(7, 0, new Vector(7, 1)),
                new Rook(8, 0, new Vector(0, 0)),
                new Rook(9, 0, new Vector(7, 0)),
                new Knight(10, 0, new Vector(1, 0)),
                new Knight(11, 0, new Vector(6, 0)),
                new Bishop(12, 0, new Vector(2, 0)),
                new Bishop(13, 0, new Vector(5, 0)),
                new Queen(14, 0, new Vector(3, 0)),
                new King(15, 0, new Vector(4, 0)),

                new Pawn(16, 1, new Vector(0, 6)),
                new Pawn(17, 1, new Vector(1, 6)),
                new Pawn(18, 1, new Vector(2, 6)),
                new Pawn(19, 1, new Vector(3, 6)),
                new Pawn(20, 1, new Vector(4, 6)),
                new Pawn(21, 1, new Vector(5, 6)),
                new Pawn(22, 1, new Vector(6, 6)),
                new Pawn(23, 1, new Vector(7, 6)),
                new Rook(24, 1, new Vector(0, 7)),
                new Rook(25, 1, new Vector(7, 7)),
                new Knight(26, 1, new Vector(1, 7)),
                new Knight(27, 1, new Vector(6, 7)),
                new Bishop(28, 1, new Vector(2, 7)),
                new Bishop(29, 1, new Vector(5, 7)),
                new Queen(30, 1, new Vector(3, 7)),
                new King(31, 1, new Vector(4, 7)),
            };
        }

        public Piece GetPieceByPosition(Vector position) {
            /*
             * Try to find a piece at a given location.
             * :param position: The position to search in.
             * :return: The Piece that was found, or null.
             */

            Piece selectedPiece = null;

            foreach (Piece piece in pieces) {
                if (piece == null) {
                    continue;
                }

                if (piece.position == position) {
                    selectedPiece = piece;
                    break;
                }
            }

            return selectedPiece;
        }

        public Vector[][] GetLegalMoves(Vector position) {
            /*
             * Get all the legal moves for a piece at the given location.
             * :param position: The location of the piece.
             * :return: A nested list: {moves, attacks}.
             */

            Piece selectedPiece = GetPieceByPosition(position);

            if (selectedPiece == null) {
                throw new NoSuchPieceException();
            }

            Vector[] moves = null;
            Vector[] attacks = null;

            if (!inCheck) {
                moves = selectedPiece.GetLegalMoves(this);
                attacks = selectedPiece.GetLegalAttacks(false, this);

            } else {
                moves = selectedPiece.GetLegalCheckMoves(this);
                attacks = selectedPiece.GetLegalCheckAttacks(this);
            }

            return new Vector[][] {moves, attacks};
        }

        public Piece[] GetThreats(int index, Vector position) {
            /*
             * Get all the threats to a given location, excluding the piece identified with index.
             * :param index: The index of the piece to ignore.
             * :param position: The position to check.
             * :return: A list of pieces threatening the given location.
             */

            ArrayList Threats = new ArrayList();

            foreach (Piece piece in pieces) {
                if (piece == null) {
                    continue;
                }

                if (piece.index == index) {
                    continue;
                }

                bool valid = false;
                foreach (Vector attack in piece.GetLegalAttacks(true, this)) {
                    if (attack == position) {
                        valid = true;
                        break;
                    }
                }

                if (valid) {
                    Threats.Add(piece);
                }
            }

            Piece[] threats_out = new Piece[Threats.ToArray().Length];
            for (int x = 0; x < Threats.ToArray().Length; x++) {
                threats_out[x] = (Piece) (Threats.ToArray()[x]);
            }

            return threats_out;
        }

        public void CheckForCheck() {
            /*
             * Check if the king is in check
             */

            Piece king = null;
            foreach (Piece piece in pieces) {
                if (piece == null) {
                    continue;
                }

                if (piece.name == "Ki" && piece.side == turnCounter.turn) {
                    king = piece;
                    break;
                }
            }

            if (king == null) {
                throw new NoSuchPieceException();
            }

            Piece[] Threats = GetThreats(king.index, king.position);
            bool emptyPass = false;
            while (!emptyPass) {
                emptyPass = true;
                for (int x = 0; x < Threats.Length; x++) {
                    if (Threats[x] == null) {
                      continue;
                    }

                    if (Threats[x].side == king.side) {
                        Threats[x] = null;
                        emptyPass = false;
                        break;
                    }
                }
            }

            int total = 0;
            foreach (Piece threat in Threats) {
                if (threat == null) {
                    continue;
                }

                total++;
            }

            if (total != 0) {
                inCheck = true;
                threats = Threats;

            } else {
                inCheck = false;
                threats = new Piece[] {};
            }
        }

        public void CheckForCheckmate() {
            /*
             * Check if the king is in checkmate.
             */

            int numberOfMoves = 0;
            foreach (Piece piece in pieces) {
                if (piece == null) {
                    continue;
                }

                if (piece.name != "Ki" || piece.side != turnCounter.turn) {
                    continue;
                }

                if (piece.side == turnCounter.turn) {
                    Vector[][] movesAndAttacks = GetLegalMoves(piece.position);
                    numberOfMoves += (movesAndAttacks[0].Length + movesAndAttacks[1].Length);
                }
            }

            checkmate = numberOfMoves == 0;
        }

        public bool MovePiece(Vector position, Vector newPosition) {
            /*
             * Try to move the piece at the given position to the new position.
             * :param position: The position of the piece to move.
             * :param newPosition: The position to move the piece to.
             * :return: True/False, depending on whether or not the move was successful.
             */

            Piece selectedPiece = GetPieceByPosition(position);

            if (selectedPiece == null) {
                throw new NoSuchPieceException();
            }

            if (selectedPiece.side != turnCounter.turn) {
                return false;
            }

            bool result = selectedPiece.Move(newPosition, this);

            if (!result) {
                Piece attackedPiece = GetPieceByPosition(newPosition);

                if (attackedPiece == null) {
                    return result;

                } else {
                    result = selectedPiece.Attack(newPosition, attackedPiece, this);
                    if (result) {
                        for (int x = 0; x < pieces.Length; x++) {
                            if (pieces[x] == null) {
                                continue;
                            }

                            if (pieces[x].index == attackedPiece.index) {
                                pieces[x] = null;
                                break;
                            }
                        }
                    }
                }
            }

            for (int x = 0; x < pieces.Length; x++) {
                if (pieces[x] == null) {
                    continue;
                }

                if (pieces[x].ShouldPromote()) {
                    pieces[x] = new Queen(pieces[x].index, pieces[x].side, pieces[x].position);
                }
            }

            if (result) {
                turnCounter.Toggle();
            }

            CheckForCheck();
            if (inCheck) {
                CheckForCheckmate();
            }

            return result;
        }
    }
}
