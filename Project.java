import java.util.*;
import java.io.Console;

class Reversi {
    Board board = new Board("black");
    static Console c;

    Reversi() {
        c = System.console();
        if (c == null) {
            System.err.println("No console.");
            System.exit(1);
        }
    }
    public static void main(String[] args) {
        doTest();
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

        System.out.format("%s WINS!!! With %d points to %d", winner, max, min);
    }

    // get input
    void takeTurn() {
        board.show();
        while (true) {
            try {
                String input = c.readLine(board.currentTurn.whosTurn()+" turn, enter x y: ");
                String[] args = input.split(" ");
                int x = Integer.valueOf(args[0]);
                int y = Integer.valueOf(args[1]);
                board.place(x,y); // Throws BadInput
                board.reset();
                break;
            } catch (BadInput badInput) {
                System.out.format("%nYou can't place there Please renter %n");
            } catch (IndexOutOfBoundsException outOfRange) {
                System.out.format("%nPlay somewhere on the board!%n");
            } catch (Exception anythingElse) {
                System.out.format("%nI have no idea what you did but it was bad.%n");
            }
        }
    }

    static void doTest() {
        Board b = new Board("black");
        for (int i = 1; i < 9; i++) {
            for ( int j = 1; j < 9; j++) {
                Tile t = b.getXY(i,j);
                //System.out.println("i: "+i+" j: "+j);
                assert(t.getX() == i);
                assert(t.getY() == j);
            }
        }
        Tile t = b.getRow(2).get(4);
        assert(t.getX() == 4);
        assert(t.getY() == 2);
        Direction d = new Direction(0,1);
        assert(d.getX() == 0);
        assert(d.getY() == 1);
        Tile n = b.getNeighbor(t, d);
        assert(n.getX() == 4);
        assert(n.getY() == 3);

        t = b.getRow(5).get(5);
        assert(t.getX() == 5);
        assert(t.getY() == 5);
        d = new Direction(0,1);
        assert(d.getX() == 0);
        assert(d.getY() == 1);
        n = b.getNeighbor(t, d);
        assert(n.getX() == 5);
        assert(n.getY() == 6);

        t = b.getRow(5).get(4);
        assert(t.getX() == 4);
        assert(t.getY() == 5);
        d = new Direction(1,-1);
        assert(d.getX() == 1);
        assert(d.getY() == -1);
        n = b.getNeighbor(t, d);
        assert(n.getX() == 5);
        assert(n.getY() == 4);

        t = b.getRow(4).get(4);
        assert(t.getX() == 4);
        assert(t.getY() == 4);
        d = new Direction(0,1);
        assert(d.getX() == 0);
        assert(d.getY() == 1);
        n = b.getNeighbor(t, d);
        assert(n.getX() == 4);
        assert(n.getY() == 5);
    }
}

class BadInput extends Exception {
    static final long serialVersionUID = 0;
}

class Side {
    String face;

    Side() {
        face = "empty";
    }

    Side(String side) {
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

    public boolean compare(Side friend) {
        return face == friend.face();
    }

    public Side clone() {
        return new Side(face);
    }
}

class Direction {
    int x,y;

    Direction(int x, int y) {
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
    List<List<Tile>> rows = new ArrayList<List<Tile>>();
    public Side currentTurn;

    Board(String startingPlayer) {
        rows.add(headerRow());
        for (int i = 1; i < 9; i++) {
            rows.add(blankRow(i));
        }
        rows.add(bottomRow());
        placeStarters();
        currentTurn = new Side(startingPlayer);
    }

    void placeStarters() {
        rows.get(4).set(4,new Piece(4, 4, this, new Side("white")));
        rows.get(5).set(5,new Piece(5, 5, this, new Side("white")));
        rows.get(4).set(5,new Piece(5, 4, this, new Side("black")));
        rows.get(5).set(4,new Piece(4, 5, this, new Side("black")));
    }

    List<Tile> blankRow(int index) {
        List<Tile> temp = new ArrayList<Tile>();

        temp.add(new BorderPiece(0,index,this,index));
        for (int i = 1; i < 9; i++) {
            temp.add(new EmptySpace(i,index,this));
        }
        temp.add(new BorderPiece(9,index,this));

        return temp;
    }

    List<Tile> headerRow() {
        List<Tile> temp = new ArrayList<Tile>();

        temp.add(new BorderPiece(0,0,this));
        for (int i = 1; i < 9; i++) {
            temp.add(new BorderPiece(i,0,this,i));
        }
        temp.add(new BorderPiece(9,0,this));

        return temp;
    }

    List<Tile> bottomRow() {
        List<Tile> temp = new ArrayList<Tile>();

        for (int i = 0; i < 11; i++) {
            temp.add(new BorderPiece(i,9,this));
        }

        return temp;
    }
    /* Usable functions */

    public Tile getNeighbor(Tile client, Direction dir) {
        return rows.get(client.getY() + dir.getY()).get(client.getX() + dir.getX());
    }

    public void place(int x, int y) throws BadInput{
        if (getXY(x,y).getClass() == EmptySpace.class) {
            EmptySpace empty = (EmptySpace) rows.get(y).get(x);
            if (empty.isMarked()) {
                empty.flipPieces();
                rows.get(y).set(x, new Piece(x, y, this, currentTurn.clone()));
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
        return rows.get(index);
    }

    public Tile getXY(int x, int y) {
        return rows.get(y).get(x);
    }

    public void show() {
        for (int i = 0; i < 100; i++) {
            System.out.println("");
        }
        System.out.flush();
        for (List<Tile> row: rows) {
            for (Tile tile: row) {
                tile.show();
            }
            System.out.format("%n");
        }
        int[] counts = piecesCount();
        System.out.format("Num of Black pieces: %d White pieces: %d%n", counts[0], counts[1]);
    }

    public void reset() {
        for (List<Tile> row: rows) {
            for (Tile tile: row) {
                tile.reset();
            }
        }
    }

    public void render() {
        for (int row = 1; row < 9; row++) {
            for (int element = 1; element < 9; element++) {
                Tile tile = rows.get(row).get(element);
                if (tile.getSide().compare(currentTurn)) {
                    Piece thingy = (Piece) tile;
                    //System.out.println("home"+thingy.getX()+" "+thingy.getY());
                    thingy.warnNeighbors();
                    System.out.println("");
                }
            }
        }
    }

    // Checks to see if the board is still playable
    // should be called after render
    public boolean playable() {
        for (int row = 1; row < 9; row++) {
            for (int element = 1; element < 9; element++) {
                Tile tile = rows.get(row).get(element);
                if (tile.getClass() == EmptySpace.class) {
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
                Tile tile = rows.get(row).get(element);
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
    int x,y;
    String face = "  ";
    Board board;
    Side side;

    Tile(int x, int y, Board board) {
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

    public void show() {
        System.out.print(face);
    }

    public void reset() {}
    public void flip(Direction dir) {}
}

class BorderPiece extends Tile {
    BorderPiece(int x, int y, Board board) {
        super(x,y,board);
    }

    BorderPiece(int x, int y, Board board, int num) {
        super(x,y,board);
        face = " "+num;
    }
}

class EmptySpace extends Tile {
    boolean marked = false;
    List<Direction> dangerDirections = new ArrayList<Direction>();

    EmptySpace(int x, int y, Board board) {
        super(x,y,board);
        side = new Side();
    }

    public boolean isMarked() {
        return marked;
    }

    public void reset() {
        dangerDirections.clear();
        marked = false;
    }

    public void mark(Direction dir) {
        marked = true;
        dangerDirections.add(dir);
    }

    public void show() {
        if (marked) System.out.print(" *");
        else super.show();
    }

    public void flipPieces() {
        for (Direction dir : dangerDirections) {
            dir.flipDirection();
            board.getNeighbor(this, dir).flip(dir);
        }
    }
}

class Piece extends Tile{

    Piece(int x, int y, Board board, Side side) {
        super(x,y,board);
        this.side = side;
    }

    public void show() {
        System.out.print(side.getFace());
    }

    public void flip(Direction dir) {
        if (!side.compare(board.currentTurn)) {
            side.flip();
            board.getNeighbor(this, dir).flip(dir);
        }
    }

    public void warnNeighbor(Direction dir) {
        // check if this is of the same face as the turn
        if (!side.compare(board.currentTurn)) {
            // Check class of the next neighbor
            // if it's an empty call the empty method
            if (board.getNeighbor(this, dir).getClass().equals(EmptySpace.class)) {
                EmptySpace temp = (EmptySpace) board.getNeighbor(this, dir);
                //System.out.println("Marking Empty"+temp.getX()+" "+temp.getY());
                temp.mark(dir);
            }
            // if it's a Piece
            else if (board.getNeighbor(this, dir).getClass().equals(Piece.class)) {
                Piece temp = (Piece) board.getNeighbor(this, dir);
                //System.out.println("warning Piece"+temp.getX()+" "+temp.getY());
                temp.warnNeighbor(dir);
            }
        }
    }

    public void warnNeighbors() {
        Board b = new Board("black");
        Tile t = b.getRow(4).get(4);
        assert(t.getX() == 4);
        assert(t.getY() == 4);
        Direction d = new Direction(0,1);
        assert(d.getX() == 0);
        assert(d.getY() == 1);
        Tile n = b.getNeighbor(t, d);
        assert(n.getX() == 4);
        assert(n.getY() == 5);

        t = board.getRow(4).get(4);
        assert(t.getX() == 4);
        assert(t.getY() == 4);
        d = new Direction(0,1);
        assert(d.getX() == 0);
        assert(d.getY() == 1);
        n = board.getNeighbor(t, d);
        assert(n.getX() == 4);
        assert(n.getY() == 5);

        //System.out.println("this: "+this.getX()+" "+this.getY());
        // Top and bottom neighbors
        for (int y = -1; y <= 1; y+=2) {
            for (int x = -1; x <= 1; x++) {
                Direction dir = new Direction(x, y);
                if (board.getNeighbor(this, dir).getClass().equals(this.getClass())) {
                    Piece temp = (Piece) board.getNeighbor(this, dir);
                    //System.out.println("; Neighbor: "+temp.getX()+" "+temp.getY()+"; Dir: "+dir.getX()+" "+dir.getY());
                    temp.warnNeighbor(dir);
                }
            }
        }
        // Middle neighbors
        for (int x = -1; x <= 1; x+=1) {
            Direction dir = new Direction(x, 0);
            if (board.getNeighbor(this, dir).getClass().equals(this.getClass())) {
                Piece temp = (Piece) board.getNeighbor(this, dir);
                //System.out.println("; Neighbor: "+temp.getX()+" "+temp.getY()+"; Dir: "+dir.getX()+" "+dir.getY());
                temp.warnNeighbor(dir);
            }
        }
    }
}
