using System;
using System.Collections.Generic;

internal class Reversi
{
	internal Board board = new Board("black");
	internal static Console c;

	internal Reversi()
	{
		c = System.console();
		if (c == null)
		{
			Console.Error.WriteLine("No console.");
			Environment.Exit(1);
		}
	}
	public static void Main(string[] args)
	{
		doTest();
		Reversi test = new Reversi();
		test.play();
	}

	internal virtual void play()
	{
		board.render();
		do
		{
			takeTurn();
			board.render();
		} while (board.playable());
		endScreen();
	}

	internal virtual void endScreen()
	{
		int[] counts = board.piecesCount();
		string winner;
		int max, min;
		if (counts[0] > counts[1])
		{
			winner = "Black";
			max = counts[0];
			min = counts[1];
		}
		else
		{
			winner = "White";
			max = counts[1];
			min = counts[0];
		}

		//System.out.format("%s WINS!!! With %d points to %d", winner, max, min);
	}

	// get input
	internal virtual void takeTurn()
	{
		board.show();
		while (true)
		{
			//try {
				string input = c.readLine(board.currentTurn.whosTurn() + " turn, enter x y: ");
				string[] args = input.Split(" ", true);
				int x = Convert.ToInt32(args[0]);
				int y = Convert.ToInt32(args[1]);
				board.place(x,y); // Throws BadInput
				board.reset();
				break;
			/*
			} catch (BadInput badInput) {
			    System.out.format("%nYou can't place there Please renter %n");
			} catch (IndexOutOfBoundsException outOfRange) {
			    System.out.format("%nPlay somewhere on the board!%n");
			} catch (Exception anythingElse) {
			    System.out.format("%nI have no idea what you did but it was bad.%n");
			}
			*/
		}
	}

	internal static void doTest()
	{
		Board b = new Board("black");
		for (int i = 1; i < 9; i++)
		{
			for (int j = 1; j < 9; j++)
			{
				Tile t = b.getXY(i,j);
				//System.out.println("i: "+i+" j: "+j);
				assert(t.X == i);
				assert(t.Y == j);
			}
		}
		Tile t = b.getRow(2)[4];
		assert(t.X == 4);
		assert(t.Y == 2);
		Direction d = new Direction(0,1);
		assert(d.X == 0);
		assert(d.Y == 1);
		Tile n = b.getNeighbor(t, d);
		assert(n.X == 4);
		assert(n.Y == 3);

		t = b.getRow(5)[5];
		assert(t.X == 5);
		assert(t.Y == 5);
		d = new Direction(0,1);
		assert(d.X == 0);
		assert(d.Y == 1);
		n = b.getNeighbor(t, d);
		assert(n.X == 5);
		assert(n.Y == 6);

		t = b.getRow(5)[4];
		assert(t.X == 4);
		assert(t.Y == 5);
		d = new Direction(1,-1);
		assert(d.X == 1);
		assert(d.Y == -1);
		n = b.getNeighbor(t, d);
		assert(n.X == 5);
		assert(n.Y == 4);

		t = b.getRow(4)[4];
		assert(t.X == 4);
		assert(t.Y == 4);
		d = new Direction(0,1);
		assert(d.X == 0);
		assert(d.Y == 1);
		n = b.getNeighbor(t, d);
		assert(n.X == 4);
		assert(n.Y == 5);
	}
}

/*
class BadInput extends Exception {
    static final long serialVersionUID = 0;
}
*/

internal class Side
{
	internal string face;

	internal Side()
	{
		face = "empty";
	}

	internal Side(string side)
	{
		if (string.ReferenceEquals(side, "black"))
		{
			face = " X";
		}
		else if (string.ReferenceEquals(side, "white"))
		{
			face = " O";
		}
		else
		{
			face = side;
		}
	}

	public virtual string whosTurn()
	{
		return (string.ReferenceEquals(face, " X")) ? "black's" : "white's";
	}

	public virtual string Face
	{
		get
		{
			return face;
		}
		set
		{
			face = value;
		}
	}


	public virtual void flip()
	{
		face = (string.ReferenceEquals(face, " X")) ? " O" : " X";
	}

	public virtual bool compare(Side friend)
	{
		return string.ReferenceEquals(face, friend.Face);
	}

	public virtual Side clone()
	{
		return new Side(face);
	}
}

internal class Direction
{
	internal int x, y;

	internal Direction(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public virtual void flipDirection()
	{
		x = -x;
		y = -y;
	}

	public virtual int X
	{
		get
		{
			return x;
		}
	}

	public virtual int Y
	{
		get
		{
			return y;
		}
	}
}

internal class Board
{
	internal IList<IList<Tile>> rows = new List<IList<Tile>>();
	public Side currentTurn;

	internal Board(string startingPlayer)
	{
		rows.Add(headerRow());
		for (int i = 1; i < 9; i++)
		{
			rows.Add(blankRow(i));
		}
		rows.Add(bottomRow());
		placeStarters();
		currentTurn = new Side(startingPlayer);
	}

	internal virtual void placeStarters()
	{
		rows[4][4] = new Piece(4, 4, this, new Side("white"));
		rows[5][5] = new Piece(5, 5, this, new Side("white"));
		rows[4][5] = new Piece(5, 4, this, new Side("black"));
		rows[5][4] = new Piece(4, 5, this, new Side("black"));
	}

	internal virtual IList<Tile> blankRow(int index)
	{
		IList<Tile> temp = new List<Tile>();

		temp.Add(new BorderPiece(0,index,this,index));
		for (int i = 1; i < 9; i++)
		{
			temp.Add(new EmptySpace(i,index,this));
		}
		temp.Add(new BorderPiece(9,index,this));

		return temp;
	}

	internal virtual IList<Tile> headerRow()
	{
		IList<Tile> temp = new List<Tile>();

		temp.Add(new BorderPiece(0,0,this));
		for (int i = 1; i < 9; i++)
		{
			temp.Add(new BorderPiece(i,0,this,i));
		}
		temp.Add(new BorderPiece(9,0,this));

		return temp;
	}

	internal virtual IList<Tile> bottomRow()
	{
		IList<Tile> temp = new List<Tile>();

		for (int i = 0; i < 11; i++)
		{
			temp.Add(new BorderPiece(i,9,this));
		}

		return temp;
	}
	/* Usable functions */

	public virtual Tile getNeighbor(Tile client, Direction dir)
	{
		return rows[client.Y + dir.Y][client.X + dir.X];
	}

	public virtual void place(int x, int y) BadInput
	{
		if (getXY(x,y).GetType() == typeof(EmptySpace))
		{
			EmptySpace empty = (EmptySpace) rows[y][x];
			if (empty.Marked)
			{
				empty.flipPieces();
				rows[y][x] = new Piece(x, y, this, currentTurn.clone());
				currentTurn.flip();
			}
			else
			{
			 // Raise error
			 //throw new BadInput();
			}
		}
		else
		{
			// Raise error
			//throw new BadInput();
		}
	}

	public virtual IList<Tile> getRow(int index)
	{
		return rows[index];
	}

	public virtual Tile getXY(int x, int y)
	{
		return rows[y][x];
	}

	public virtual void show()
	{
		for (int i = 0; i < 100; i++)
		{
			//System.out.println("");
		}
		//System.out.flush();
		foreach (IList<Tile> row in rows)
		{
			foreach (Tile tile in row)
			{
				tile.show();
			}
			//System.out.format("%n");
		}
		int[] counts = piecesCount();
		//System.out.format("Num of Black pieces: %d White pieces: %d%n", counts[0], counts[1]);
	}

	public virtual void reset()
	{
		foreach (IList<Tile> row in rows)
		{
			foreach (Tile tile in row)
			{
				tile.reset();
			}
		}
	}

	public virtual void render()
	{
		for (int row = 1; row < 9; row++)
		{
			for (int element = 1; element < 9; element++)
			{
				Tile tile = rows[row][element];
				if (tile.Side.compare(currentTurn))
				{
					Piece thingy = (Piece) tile;
					//System.out.println("home"+thingy.getX()+" "+thingy.getY());
					thingy.warnNeighbors();
					//System.out.println("");
				}
			}
		}
	}

	// Checks to see if the board is still playable
	// should be called after render
	public virtual bool playable()
	{
		for (int row = 1; row < 9; row++)
		{
			for (int element = 1; element < 9; element++)
			{
				Tile tile = rows[row][element];
				if (tile.GetType() == typeof(EmptySpace))
				{
					EmptySpace empty = (EmptySpace) tile;
					if (empty.Marked)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public virtual int[] piecesCount()
	{
		int[] countPieces = new int[] {0, 0};
		for (int row = 1; row < 9; row++)
		{
			for (int element = 1; element < 9; element++)
			{
				Tile tile = rows[row][element];
				if (string.ReferenceEquals(tile.Side.Face, " X"))
				{
					countPieces[0]++;
				}
				else if (string.ReferenceEquals(tile.Side.Face, " O"))
				{
					countPieces[1]++;
				}
			}
		}
		return countPieces;
	}
}

internal class Tile
{
	internal int x, y;
	internal string face = "  ";
	internal Board board;
	internal Side side;

	internal Tile(int x, int y, Board board)
	{
		this.x = x;
		this.y = y;
		this.board = board;
	}

	public virtual int X
	{
		get
		{
			return x;
		}
	}

	public virtual int Y
	{
		get
		{
			return y;
		}
	}

	public virtual Side Side
	{
		get
		{
			return side;
		}
	}

	public virtual void show()
	{
		//System.out.print(face);
	}

	public virtual void reset()
	{
	}
	public virtual void flip(Direction dir)
	{
	}
}

internal class BorderPiece : Tile
{
	internal BorderPiece(int x, int y, Board board) : base(x,y,board)
	{
	}

	internal BorderPiece(int x, int y, Board board, int num) : base(x,y,board)
	{
		face = " " + num;
	}
}

internal class EmptySpace : Tile
{
	internal bool marked = false;
	internal IList<Direction> dangerDirections = new List<Direction>();

	internal EmptySpace(int x, int y, Board board) : base(x,y,board)
	{
		side = new Side();
	}

	public virtual bool Marked
	{
		get
		{
			return marked;
		}
	}

	public override void reset()
	{
		dangerDirections.Clear();
		marked = false;
	}

	public virtual void mark(Direction dir)
	{
		marked = true;
		dangerDirections.Add(dir);
	}

	/*
	public void show() {
	    if (marked) System.out.print(" *");
	    else super.show();
	}
	*/

	public virtual void flipPieces()
	{
		foreach (Direction dir in dangerDirections)
		{
			dir.flipDirection();
			board.getNeighbor(this, dir).flip(dir);
		}
	}
}

internal class Piece : Tile
{

	internal Piece(int x, int y, Board board, Side side) : base(x,y,board)
	{
		this.side = side;
	}

	public override void show()
	{
		//System.out.print(side.getFace());
	}

	public override void flip(Direction dir)
	{
		if (!side.compare(board.currentTurn))
		{
			side.flip();
			board.getNeighbor(this, dir).flip(dir);
		}
	}

	public virtual void warnNeighbor(Direction dir)
	{
		// check if this is of the same face as the turn
		if (!side.compare(board.currentTurn))
		{
			// Check class of the next neighbor
			// if it's an empty call the empty method
			if (board.getNeighbor(this, dir).GetType().Equals(typeof(EmptySpace)))
			{
				EmptySpace temp = (EmptySpace) board.getNeighbor(this, dir);
				//System.out.println("Marking Empty"+temp.getX()+" "+temp.getY());
				temp.mark(dir);
			}
			// if it's a Piece
			else if (board.getNeighbor(this, dir).GetType().Equals(typeof(Piece)))
			{
				Piece temp = (Piece) board.getNeighbor(this, dir);
				//System.out.println("warning Piece"+temp.getX()+" "+temp.getY());
				temp.warnNeighbor(dir);
			}
		}
	}

	public virtual void warnNeighbors()
	{
		Board b = new Board("black");
		Tile t = b.getRow(4)[4];
		assert(t.X == 4);
		assert(t.Y == 4);
		Direction d = new Direction(0,1);
		assert(d.X == 0);
		assert(d.Y == 1);
		Tile n = b.getNeighbor(t, d);
		assert(n.X == 4);
		assert(n.Y == 5);

		t = board.getRow(4)[4];
		assert(t.X == 4);
		assert(t.Y == 4);
		d = new Direction(0,1);
		assert(d.X == 0);
		assert(d.Y == 1);
		n = board.getNeighbor(t, d);
		assert(n.X == 4);
		assert(n.Y == 5);

		//System.out.println("this: "+this.getX()+" "+this.getY());
		// Top and bottom neighbors
		for (int y = -1; y <= 1; y += 2)
		{
			for (int x = -1; x <= 1; x++)
			{
				Direction dir = new Direction(x, y);
				if (board.getNeighbor(this, dir).GetType().Equals(this.GetType()))
				{
					Piece temp = (Piece) board.getNeighbor(this, dir);
					//System.out.println("; Neighbor: "+temp.getX()+" "+temp.getY()+"; Dir: "+dir.getX()+" "+dir.getY());
					temp.warnNeighbor(dir);
				}
			}
		}
		// Middle neighbors
		for (int x = -1; x <= 1; x += 1)
		{
			Direction dir = new Direction(x, 0);
			if (board.getNeighbor(this, dir).GetType().Equals(this.GetType()))
			{
				Piece temp = (Piece) board.getNeighbor(this, dir);
				//System.out.println("; Neighbor: "+temp.getX()+" "+temp.getY()+"; Dir: "+dir.getX()+" "+dir.getY());
				temp.warnNeighbor(dir);
			}
		}
	}
}
