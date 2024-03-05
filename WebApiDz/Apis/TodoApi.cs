using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace WebApiDz.Apis;

public class TodoApi
{
    private static List<Todo> _todoList;

    static TodoApi()
    {
        _todoList = new List<Todo>()
        {
            new Todo() { Id = 1, Label = "Task 1", IsDone = true },
            new Todo() { Id = 2, Label = "Task 2", IsDone = false },
            new Todo() { Id = 3, Label = "Task 3", IsDone = true },
            new Todo() { Id = 4, Label = "Task 4", IsDone = true },
            new Todo() { Id = 5, Label = "Task 5", IsDone = false }
        };
    }

    public static void Map(WebApplication app)
    {
        app.MapGet("/todos", (int limit, int offset) =>
        {
            var todos = _todoList.OrderBy(x => x.Id).Skip(offset).Take(limit).ToList();
            return todos;
        });

        app.MapGet("/todos/{id}", Results<Ok<Todo>, NotFound> (int id) =>
        {
            var todo = _todoList.SingleOrDefault(x => x.Id == id);
            if (todo != null)
            {
                return TypedResults.Ok(todo);
            }

            return TypedResults.NotFound();
        });

        app.MapGet("/todos/{id}/isDone", (int id) =>
        {
            var todo = _todoList.FirstOrDefault(x => x.Id == id);
            if (todo == null)
            {
                return Results.NotFound(new { message = "Todo item not found" });
            }

            return Results.Json(new { todo.Id, todo.IsDone });
        });

        app.MapPost("todos", (Todo newTodo, HttpContext http) =>
        {
            if (newTodo.Id == 0)
            {
                newTodo.Id = _todoList.Count != 0 ? _todoList.Max(x => x.Id) + 1 : 1;
            }
            else if (_todoList.Exists(x => x.Id == newTodo.Id))
            {
                return Results.NotFound(new { message = "This ID is already in the list" });
            }

            _todoList.Add(newTodo);

            var url = $"/todos/{newTodo.Id}";
            return Results.Created(url, newTodo);
        });

        app.MapPut("/todos/{id}", (int id, Todo todoItem, HttpContext httpContext) =>
        {
            var todo = _todoList.FirstOrDefault(x => x.Id == id);
            if (todo == null)
            {
                httpContext.Response.StatusCode = 404;
                return Results.NotFound(new { message = "Todo item not found" });
            }

            todo.Label = todoItem.Label;
            todo.IsDone = todoItem.IsDone;
            todo.UpdatedDate = DateTime.UtcNow;

            return Results.Ok(todo);
        });

        app.MapPatch("/todos/{id}", (int id, [FromBody] bool isDone,  HttpContext httpContext) =>
        {
            var todoItem = _todoList.FirstOrDefault(x => x.Id == id);
            if (todoItem == null)
            {
                httpContext.Response.StatusCode = 404;
                return Results.NotFound(new { message = "Todo item not found" });
            }

            todoItem.IsDone = isDone;
            return Results.Ok(new { id = todoItem.Id, IsDone = todoItem.IsDone });
        });

        app.MapDelete("/books/{id}", (int id) =>
        {
            _todoList = _todoList.Where(x => x.Id != id).ToList();
            return Results.Ok();
        });
    }
}

public class Todo
{
    public int Id { get; set; }
    public string Label { get; set; } = default!;
    public bool IsDone { get; set; }
    public DateTime CreatedDate { get; }
    public DateTime UpdatedDate { get; set; }

    public Todo()
    {
        CreatedDate = DateTime.Now;
        UpdatedDate = CreatedDate;
    }
}