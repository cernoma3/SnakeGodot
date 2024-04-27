using Godot;
using System.Collections.Generic;
using System;

public partial class Game : Node2D
{
	private List<Vector2> snakePositions = new List<Vector2>();
	private Vector2 snakeDirection = new Vector2(1, 0);
	private double timeSinceLastMove = 0.0f;
	private const double initialMoveInterval = 0.3;
	private const double intervalDecrement = 0.02;
	private double moveInterval = 0.3f;
	private const int CellSize = 20;
	private Vector2 berryPosition;
	private Rect2 playArea;
	private const int borderThickness = 20;
	private int score = 0;
	private int lastScore = 0;
	
	private const int scoreThreshold = 5;

	public override void _Ready()
	{
		playArea = new Rect2(Vector2.Zero, GetViewportRect().Size - new Vector2(1, 1) * CellSize);
		snakePositions.Add(new Vector2(5, 5));
		PlaceBerry();
	}

	public override void _Process(double delta)
	{
		ProcessInput();
		timeSinceLastMove += delta;

		if (timeSinceLastMove >= moveInterval)
		{
			MoveSnake();
			timeSinceLastMove = 0.0f;
			
			QueueRedraw();

			GD.Print($"Snake position: {snakePositions[0]}, Berry position: {berryPosition}, Move interval: {moveInterval}");
			GD.Print("Viewport size: ", GetViewport().GetVisibleRect().Size);
		}
	}

	public override void _Draw()
	{
		DrawSnake();
		DrawBerry();
		DrawBorders();
		DrawScore();
	}
	
	private void ProcessInput()
	{
		if (Input.IsActionPressed("ui_right") && snakeDirection != new Vector2(-1, 0))
			snakeDirection = new Vector2(1, 0);
		if (Input.IsActionPressed("ui_left") && snakeDirection != new Vector2(1, 0))
			snakeDirection = new Vector2(-1, 0);
		if (Input.IsActionPressed("ui_up") && snakeDirection != new Vector2(0, 1))
			snakeDirection = new Vector2(0, -1);
		if (Input.IsActionPressed("ui_down") && snakeDirection != new Vector2(0, -1))
			snakeDirection = new Vector2(0, 1);
	}
	
	private void MoveSnake()
	{
		Vector2 newHeadPosition = snakePositions[0] + snakeDirection;

		if (newHeadPosition == berryPosition)
		{
			snakePositions.Insert(0, newHeadPosition);
			++score;
			UpdateMoveInterval();
			PlaceBerry();
		}
		else
		{
			snakePositions.Insert(0, newHeadPosition);
			snakePositions.RemoveAt(snakePositions.Count - 1);
		}

		if (CheckCollision(newHeadPosition))
		{
			ResetGame();
		}
	}
	
	private bool CheckCollision(Vector2 headPosition)
	{
		if (headPosition.X < borderThickness / CellSize || headPosition.Y < borderThickness / CellSize ||
		headPosition.X >= GetViewport().GetVisibleRect().Size.X / CellSize - borderThickness / CellSize ||
		headPosition.Y >= GetViewport().GetVisibleRect().Size.Y / CellSize - borderThickness / CellSize ||
		snakePositions.IndexOf(headPosition, 1) != -1)
		{
			return true;
		}

		return false;
	}
	
	private void DrawSnake()
	{
		foreach (Vector2 pos in snakePositions)
		{
			DrawRect(new Rect2(pos * CellSize, new Vector2(CellSize, CellSize)), Colors.Green, true);
		}
	}
	
	private void DrawBerry()
	{
		var foodRect = new Rect2(berryPosition * CellSize, new Vector2(CellSize, CellSize));
		DrawRect(foodRect, Colors.Red, true);
		GD.Print($"Berry position: {berryPosition}");
	}
	
	private void DrawBorders()
	{
		var screenSize = GetViewportRect().Size;

		DrawRect(new Rect2(0, 0, screenSize.X, borderThickness), Colors.White, true);
		DrawRect(new Rect2(0, screenSize.Y - borderThickness, screenSize.X, borderThickness), Colors.White, true);
		DrawRect(new Rect2(0, 0, borderThickness, screenSize.Y), Colors.White, true);
		DrawRect(new Rect2(screenSize.X - borderThickness, 0, borderThickness, screenSize.Y), Colors.White, true);
	}

	private void PlaceBerry()
	{
		Random random = new Random();

		int maxX = (int)(GetViewport().GetVisibleRect().Size.X / CellSize) - (borderThickness / CellSize) * 2;
		int maxY = (int)(GetViewport().GetVisibleRect().Size.Y / CellSize) - (borderThickness / CellSize) * 2;

		Vector2 newBerryPosition;
		do
		{
			int x = random.Next(maxX) + (borderThickness / CellSize);
			int y = random.Next(maxY) + (borderThickness / CellSize);
			newBerryPosition = new Vector2(x, y) / CellSize;
		} while (snakePositions.Contains(newBerryPosition * CellSize));

		berryPosition = newBerryPosition * CellSize;
		GD.Print("New berry added at: ", berryPosition);
	}
	
	private void DrawScore()
	{
		DrawString(ThemeDB.FallbackFont, new Vector2(5, 15), 
		$"Score: {score}, Last Score: {lastScore}", 
		HorizontalAlignment.Center,-1,18, Colors.Blue);
	}
	
	private void ResetGame()
	{
		lastScore = score;
		score = 0;
		snakePositions.Clear();
		snakePositions.Add(new Vector2(5, 5));
		snakeDirection = new Vector2(1, 0);
		moveInterval = initialMoveInterval;
		PlaceBerry();
	}

	private void UpdateMoveInterval()
	{
		moveInterval = initialMoveInterval - (Math.Floor((double)score / scoreThreshold) * intervalDecrement);

		if (moveInterval < 0.1)
		{
			 moveInterval = 0.1;
		}
	}
}
