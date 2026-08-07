using System.Globalization;
using System.Security.Cryptography;

Random rnd = new Random();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()  
              .AllowAnyMethod()  
              .AllowAnyHeader();  
    });
});

builder.Services.AddSingleton<listRepository>();

var app = builder.Build();

app.UseCors("AllowAll");

app.MapPost("/start", (listRepository repo) =>
{
    string gameID = Guid.NewGuid().ToString();
    repo.gamesList.Add(gameID, new Game());

    Console.WriteLine($"{gameID} enter Start");
    return Results.Ok(new {GameID = gameID});
});

app.MapPost("/step", (listRepository repo, StepReq req) =>
{
    Console.WriteLine("start api");
    string id = req.id;
    int x = req.x;
    int y = req.y;

    if (!repo.gamesList.TryGetValue(id, out var game))
    {
        Console.WriteLine("NotFoundGame");
        return Results.NotFound("NotFoundGame");
    }

    StepResult resStep = game.Step(x, y);

    if (resStep == StepResult.CellOccupied)
    {
        Console.WriteLine("CellOccupied");
        return Results.BadRequest(new { status = "CellOccupied" });
    }
    else if (resStep == StepResult.Error)
    {
        Console.WriteLine("Error");
        return Results.BadRequest(new { status = "Error" });
    }
    else if (resStep == StepResult.Draw)
    {
        Console.WriteLine("Draw");
        return Results.Ok(new { status = "Draw", board = game.GetBoard() });
    }
    else if (resStep == StepResult.XWin)
    {
        Console.WriteLine("XWin");
        return Results.Ok(new { status = "XWin", board = game.GetBoard() });
    }

    StepResult botStepRes = game.BotStep();

    if (botStepRes == StepResult.Draw)
    {
        Console.WriteLine("Bot Step: Draw");
        return Results.Ok(new { status = "Draw", board = game.GetBoard() });
    }
    else if (botStepRes == StepResult.OWin)
    {
        Console.WriteLine("Bot Step: OWin");
        return Results.Ok(new { status = "OWin", board = game.GetBoard() });
    }

    Console.WriteLine("Success");
    return Results.Ok(new { status = "Success", board = game.GetBoard() });
});

app.Run();

public class listRepository
{
    public Dictionary<string, Game> gamesList = new Dictionary<string, Game>();
}

public enum StepResult
{
    Success,      
    CellOccupied, 
    Draw,         
    XWin,
    OWin,
    Error
}
public class Game
{
    public int[,] grid = new int[3,3];

    int numberSteps = 0;

    public StepResult Step(int x, int y)
    {
        if (grid[x,y]==0)
        {
            grid[x,y]=1;
            numberSteps += 1;

            if (checkwin(1))
            {
                return StepResult.XWin;
            }
            if (numberSteps >= 9)
            {
                return StepResult.Draw;
            }
            
            return StepResult.Success;
        }
        else if (grid[x, y] == 1 || grid[x, y] == 2)
        {
            return StepResult.CellOccupied;
        }
        else
        {
            return StepResult.Error;
        }
    }

    public StepResult BotStep()
    {
        
        int x = new int();
        int y = new int();
        if (numberSteps<9) do
        {
            x = Random.Shared.Next(0, 3);
            y = Random.Shared.Next(0, 3);
        }
        while(grid[x,y]!=0);
        grid[x,y]=2;
        numberSteps++;
        if (checkwin(2))
        {
            return StepResult.OWin;
        }
        return StepResult.Success;
    }
    public bool checkwin (int sign)
    {
        for (int i=0; i < 3; i++)
        {
            if(grid[i,0]==sign && grid[i,1]==sign && grid[i, 2] == sign)
                return true;
            if(grid[0,i]==sign && grid[1,i]==sign && grid[2,i]==sign)
                return true;
        }

        if(grid[0,0]==sign && grid[1,1]==sign && grid[2,2]==sign)
            return true;
        if(grid[0,2]==sign && grid[1,1]==sign && grid[2,0]==sign)
            return true;

        return false;
    }

    public int[][] GetBoard()
    {
        return new int[][]
        {
            new int[] { grid[0, 0], grid[0, 1], grid[0, 2] },
            new int[] { grid[1, 0], grid[1, 1], grid[1, 2] },
            new int[] { grid[2, 0], grid[2, 1], grid[2, 2] }
        };
    }
}
public record StepReq(int x, int y, string id);
public record startReq(bool start);