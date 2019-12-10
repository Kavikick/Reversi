using System;
using System.Collections.Generic;

namespace Project
{

class Reversi {
    Board board = new Board("black");

    static void Main(string[] args) {
        Console.WriteLine("HelloWorld");
        Reversi test = new Reversi();
        test.play();
    }

    void play() {
        board.render();
        do {
            takeTurn();
            board.render();
        } while (board.playable());
        endScreen();
    }

    void endScreen() {
        int[] counts = board.piecesCount();
        String winner;
        int max,min;
        if (counts[0] > counts[1]) {
            winner = "Black";
            max = counts[0];
            min = counts[1];
        } else {
            winner = "White";
            max = counts[1];
            min = counts[0];
        }

        Console.WriteLine($"{winner} WINS!!! With {max} points to {min}");
    }

    // get input
    void takeTurn() {
        board.show();
        while (true) {
            try {
                Console.Write(board.currentTurn.whosTurn()+" turn, enter x y: ");
                String input = Console.ReadLine();
                String[] args = input.Split(" ");
                int x = Convert.ToInt32(args[0]);
                int y = Convert.ToInt32(args[1]);
                board.place(x,y); // Throws BadInput
                board.reset();
                break;
            } catch (BadInput badInput) {
                Console.WriteLine("\nYou can't place there Please renter \n");
            } catch (ArgumentOutOfRangeException sheGONE) {
                Console.WriteLine("\nPick a piece on the board please");
            } catch (Exception anythingElse) {
                Console.WriteLine("\nI have no idea what you did but it was bad.\n");
            }
        }
    }
}

class BadInput : System.Exception { }

class Side {
    String face;

    public Side() {
        face = "empty";
    }

    public Side(String side) {
        if (side == "black") {
            face = " X";
        }
        else if (side == "white") {
            face = " O";
        }
        else {
            face = side;
        }
    }

    public String whosTurn() {
        return (face == " X") ? "black's" : "white's";
    }

    public String getFace() {
        return face;
    }

    public void setFace(String set) {
        face = set;
    }

    public void flip() {
        face = (face == " X") ? " O" : " X";
    }

    public bool compare(Side friend) {
        return face == friend.getFace();
    }

    public Side clone() {
        return new Side(face);
    }
}

class Direction {
    int x,y;

    public Direction(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public void flipDirection() {
        x = -x;
        y = -y;
    }
    
    public int getX() {
        return x;
    }

    public int getY() {
        return y;
    }
}

class Board {
    List<List<Tile>> rows = new List<List<Tile>>();
    public Side currentTurn;

    public Board(String startingPlayer) {
        rows.Add(headerRow());
        for (int i = 1; i < 9; i++) {
            rows.Add(blankRow(i));
        }
        rows.Add(bottomRow());
        placeStarters();
        currentTurn = new Side(startingPlayer);
    }

    void placeStarters() {
        rows[4][4] = new Piece(4, 4, this, new Side("white"));
        rows[5][5] = new Piece(5, 5, this, new Side("white"));
        rows[4][5] = new Piece(5, 4, this, new Side("black"));
        rows[5][4] = new Piece(4, 5, this, new Side("black"));
    }

    List<Tile> blankRow(int index) {
        List<Tile> temp = new List<Tile>();

        temp.Add(new BorderPiece(0,index,this,index));
        for (int i = 1; i < 9; i++) {
            temp.Add(new EmptySpace(i,index,this));
        }
        temp.Add(new BorderPiece(9,index,this));

        return temp;
    }

    List<Tile> headerRow() {
        List<Tile> temp = new List<Tile>();
        
        temp.Add(new BorderPiece(0,0,this));
        for (int i = 1; i < 9; i++) {
            temp.Add(new BorderPiece(i,0,this,i));
        }
        temp.Add(new BorderPiece(9,0,this));

        return temp;
    }

    List<Tile> bottomRow() {
        List<Tile> temp = new List<Tile>();
        
        for (int i = 0; i < 11; i++) {
            temp.Add(new BorderPiece(i,9,this));
        }

        return temp;
    }
    /* Usable functions */

    public Tile getNeighbor(Tile client, Direction dir) {
        return rows[client.getY() + dir.getY()][client.getX() + dir.getX()];
    }

    public void place(int x, int y) {
        if (getXY(x,y).isAnEmpty()) {
            EmptySpace empty = (EmptySpace) rows[y][x];
            if (empty.isMarked()) {
                empty.flipPieces();
                rows[y][x] = new Piece(x, y, this, currentTurn.clone());
                currentTurn.flip();
            } else {
             // Raise error
             throw new BadInput();
            }
        } else {
            // Raise error
            throw new BadInput();
        }
    }

    public List<Tile> getRow(int index) {
        return rows[index];
    }

    public Tile getXY(int x, int y) {
        return rows[y][x];
    }

    public void show() {
        for (int i = 0; i < 100; i++) {
            Console.WriteLine("");
        }
        Console.Clear();
        foreach (List<Tile> row in rows) {
            foreach (Tile tile in row) {
                tile.show();
            }
            Console.WriteLine("");
        }
        int[] counts = piecesCount();
        Console.WriteLine($"Num of Black pieces: {counts[0]} White pieces: {counts[1]}");
    }

    public void reset() {
        foreach (List<Tile> row in rows) {
            foreach (Tile tile in row) {
                tile.reset();
            }
        }
    }

    public void render() {
        for (int row = 1; row < 9; row++) {
            for (int element = 1; element < 9; element++) {
                Tile tile = rows[row][element];
                if (tile.getSide().compare(currentTurn)) {
                    Piece thingy = (Piece) tile;
                    //System.out.println("home"+thingy.getX()+" "+thingy.getY());
                    thingy.warnNeighbors();
                    Console.WriteLine("");
                }
            }
        }
    }

    // Checks to see if the board is still playable
    // should be called after render
    public bool playable() {
        for (int row = 1; row < 9; row++) {
            for (int element = 1; element < 9; element++) {
                Tile tile = rows[row][element];
                if (tile.isAnEmpty()) {
                    EmptySpace empty = (EmptySpace) tile;
                    if (empty.isMarked()) return true;
                }
            }
        }
        return false;
    }

    public int[] piecesCount() {
        int[] countPieces = {0,0};
        for (int row = 1; row < 9; row++) {
            for (int element = 1; element < 9; element++) {
                Tile tile = rows[row][element];
                if (tile.getSide().getFace() == " X") {
                    countPieces[0]++;
                } else if (tile.getSide().getFace() == " O") {
                    countPieces[1]++;
                }
            }
        }
        return countPieces;
    }
}

class Tile {
    protected int x,y;
    protected String face = "  ";
    protected Board board;
    protected Side side;

    public Tile(int x, int y, Board board) {
        this.x = x;
        this.y = y;
        this.board = board;
    }

    public int getX() {
        return x;
    }

    public int getY() {
        return y;
    }

    public Side getSide() {
        return side;
    }

    public virtual void show() {
        Console.Write(face);
    }

    public virtual void reset() {}
    public virtual void flip(Direction dir) {}
    public virtual void warnNeighbor(Direction dir) {}
    public virtual bool isAnEmpty() {
        return false;
    }
}

class BorderPiece : Tile {
    public BorderPiece(int x, int y, Board board) : base(x,y,board) {}

    public BorderPiece(int x, int y, Board board, int num) : base(x,y,board){
        face = " "+num;
    }
}

class EmptySpace: Tile {
    bool marked = false;
    List<Direction> dangerDirections = new List<Direction>();

    public EmptySpace(int x, int y, Board board) : base(x,y,board){
        side = new Side();
    }

    public bool isMarked() {
        return marked;
    }

    public override void reset() {
        dangerDirections.Clear();
        marked = false;
    }

    public override void warnNeighbor(Direction dir) {
        marked = true;
        dangerDirections.Add(dir);
    }

    public override void show() {
        if (marked) Console.Write(" *");
        else base.show();
    }

    public void flipPieces() {
        foreach (Direction dir in dangerDirections) {
            dir.flipDirection();
            board.getNeighbor(this, dir).flip(dir);
        }
    }

    public override bool isAnEmpty() {
        return true;
    }
}

class Piece: Tile{

    public Piece(int x, int y, Board board, Side side) : base(x,y,board){
        this.side = side;
    }

    public override void show() {
        Console.Write(side.getFace());
    }

    public override void flip(Direction dir) {
        if (!side.compare(board.currentTurn)) {
            side.flip();
            board.getNeighbor(this, dir).flip(dir);
        }
    }

    public override void warnNeighbor(Direction dir) {
        // check if this is of the same face as the turn
        if (!side.compare(board.currentTurn)) {
            board.getNeighbor(this, dir).warnNeighbor(dir);
        }
    }

    public void warnNeighbors() {
        Tile temp;
        // System.out.println("this: "+this.getX()+" "+this.getY());
        // Top and bottom neighbors
        for (int y = -1; y <= 1; y+=2) {
            for (int x = -1; x <= 1; x++) {
                Direction dir = new Direction(x, y);
                temp = board.getNeighbor(this, dir);
                if(! temp.isAnEmpty()) {
                    temp.warnNeighbor(dir);
                }
            }
        }
        // Middle neighbors
        for (int x = -1; x <= 1; x+=1) {
            Direction dir = new Direction(x, 0);
            temp = board.getNeighbor(this, dir);
            if(! temp.isAnEmpty()) {
                temp.warnNeighbor(dir);
            }
        }
    }
}


}

